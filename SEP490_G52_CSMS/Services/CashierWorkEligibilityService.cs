using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons.Constants;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class CashierWorkEligibilityService : ICashierWorkEligibilityService
    {
        private readonly CSMSAppDbContext _context;

        public CashierWorkEligibilityService(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<CashierEligibilityResult> CheckEligibilityAsync(int cashierId, string branchId)
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var nowTime = DateTime.Now.TimeOfDay;

            // 1. Operating Hours Check (06:00 to 24:00)
            if (nowTime < TimeSpan.FromHours(6))
            {
                return new CashierEligibilityResult
                {
                    IsEligible = false,
                    ReasonCode = "OutsideOperatingHours",
                    Message = "Hệ thống chỉ mở bán hàng từ 06:00 đến 24:00 hàng ngày. Hiện tại đang ngoài khung giờ phục vụ."
                };
            }

            // 2. Roster Schedule Check
            var rosters = await _context.WeeklyRosterGrids
                .Include(r => r.FixedShift)
                .Include(r => r.AttendanceLogs)
                .Where(r => r.EmployeeId == cashierId && r.BranchId == branchId && (r.AssignmentDate.Date == today || r.AssignmentDate.Date == yesterday))
                .ToListAsync();

            if (!rosters.Any())
            {
                return new CashierEligibilityResult
                {
                    IsEligible = false,
                    ReasonCode = "NotScheduled",
                    Message = "Bạn không có lịch phân công ca làm việc tại chi nhánh hôm nay. Vui lòng kiểm tra lại Lịch làm việc."
                };
            }

            // 1. Match current roster by current time (within shift bounds: start - 30m to end)
            var matchingRoster = rosters.FirstOrDefault(r => {
                if (r.FixedShift == null) return false;
                var start = r.FixedShift.StartTime;
                var end = r.FixedShift.EndTime;

                if (start <= end)
                {
                    return r.AssignmentDate.Date == today && nowTime >= start.Subtract(TimeSpan.FromMinutes(30)) && nowTime <= end;
                }
                else
                {
                    if (r.AssignmentDate.Date == today) return nowTime >= start.Subtract(TimeSpan.FromMinutes(30));
                    if (r.AssignmentDate.Date == yesterday) return nowTime <= end;
                    return false;
                }
            });

            // 2. If no roster strictly matches shift bounds, check active check-in roster whose end time hasn't expired by more than 30 mins
            if (matchingRoster == null)
            {
                matchingRoster = rosters.FirstOrDefault(r => {
                    if (r.FixedShift == null) return false;
                    var log = r.AttendanceLogs.FirstOrDefault();
                    if (log == null || log.CheckInTime == null || log.CheckOutTime != null) return false;
                    
                    var end = r.FixedShift.EndTime;
                    return r.AssignmentDate.Date == today && nowTime <= end.Add(TimeSpan.FromMinutes(30));
                });
            }

            // 3. Fallback: match by wider window or earliest shift
            if (matchingRoster == null)
            {
                var earlyWindow = TimeSpan.FromHours(2);
                matchingRoster = rosters.FirstOrDefault(r => {
                    if (r.FixedShift == null) return false;
                    var start = r.FixedShift.StartTime;
                    var end = r.FixedShift.EndTime;

                    if (start <= end)
                    {
                        return r.AssignmentDate.Date == today && nowTime >= start.Subtract(earlyWindow) && nowTime <= end.Add(TimeSpan.FromHours(1));
                    }
                    else
                    {
                        if (r.AssignmentDate.Date == today) return nowTime >= start.Subtract(earlyWindow);
                        if (r.AssignmentDate.Date == yesterday) return nowTime <= end.Add(TimeSpan.FromHours(1));
                        return false;
                    }
                }) ?? rosters.OrderBy(r => r.FixedShift?.StartTime).FirstOrDefault();
            }

            if (matchingRoster == null || matchingRoster.FixedShift == null)
            {
                return new CashierEligibilityResult
                {
                    IsEligible = false,
                    ReasonCode = "NotScheduled",
                    Message = "Hiện tại chưa tới giờ ca làm việc của bạn. Vui lòng kiểm tra lại Lịch làm việc."
                };
            }

            // Determine Shift Phase based on branch handover history today
            bool hasAnyHandoverToday = await _context.CashHandovers
                .AnyAsync(ch => ch.BranchId == branchId && ch.HandoverDate.Date == matchingRoster.AssignmentDate.Date);

            string shiftPhase;
            if (!hasAnyHandoverToday)
            {
                shiftPhase = CashHandoverConstants.HandoverTypeFirstShift;
            }
            else
            {
                var allShifts = await _context.FixedShifts.OrderBy(s => s.StartTime).ToListAsync();
                var shiftIndex = allShifts.FindIndex(s => s.ShiftId == matchingRoster.ShiftId);
                if (shiftIndex == allShifts.Count - 1)
                    shiftPhase = CashHandoverConstants.HandoverTypeLastShift;
                else
                    shiftPhase = CashHandoverConstants.HandoverTypeMidShift;
            }

            // 3. ATTENDANCE CHECK (CHẤM CÔNG - TOP PRIORITY FOR WORKING ELIGIBILITY!)
            var attendanceLog = matchingRoster.AttendanceLogs.FirstOrDefault();
            bool isCheckedIn = (attendanceLog != null && attendanceLog.CheckInTime != null && (attendanceLog.OverallStatus == "Present" || attendanceLog.CheckInStatus == "OnTime" || attendanceLog.CheckInStatus == "Late"));

            if (!isCheckedIn)
            {
                return new CashierEligibilityResult
                {
                    IsEligible = false,
                    ReasonCode = "NotCheckedIn",
                    Message = $"Bạn chưa thực hiện điểm danh chấm công vào ca {matchingRoster.FixedShift.ShiftName}. Vui lòng điểm danh chấm công trước khi mở ca và bán hàng.",
                    ActiveShiftId = matchingRoster.ShiftId,
                    ShiftName = matchingRoster.FixedShift.ShiftName,
                    ShiftPhase = shiftPhase,
                    IsCheckedIn = false,
                    IsShiftOpened = false
                };
            }

            if (attendanceLog!.CheckOutTime != null || attendanceLog.CheckOutStatus == "CheckedOut")
            {
                return new CashierEligibilityResult
                {
                    IsEligible = false,
                    ReasonCode = "ShiftEnded",
                    Message = $"Ca làm việc {matchingRoster.FixedShift.ShiftName} của bạn đã kết thúc (đã chấm công ra ca).",
                    ActiveShiftId = matchingRoster.ShiftId,
                    ShiftName = matchingRoster.FixedShift.ShiftName,
                    ShiftPhase = shiftPhase,
                    IsCheckedIn = true,
                    IsShiftOpened = false
                };
            }

            // 4. CASH HANDOVER CHECK (CHECK ACTIVE HANDOVER FOR THE BRANCH TODAY)
            var activeHandover = await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .Include(ch => ch.OutgoingCashier)
                .AsNoTracking()
                .FirstOrDefaultAsync(ch => ch.BranchId == branchId 
                                        && ch.HandoverDate.Date == matchingRoster.AssignmentDate.Date
                                        && ch.Status == CashHandoverConstants.ActiveStatus);

            if (activeHandover != null)
            {
                // Correct Cashier or Consecutive Shifts by the same Cashier
                if (activeHandover.OutgoingCashierId == cashierId || activeHandover.IncomingCashierId == cashierId)
                {
                    return new CashierEligibilityResult
                    {
                        IsEligible = true,
                        ReasonCode = "Eligible",
                        Message = "Hợp lệ",
                        ActiveShiftId = activeHandover.ShiftId,
                        ShiftName = matchingRoster.FixedShift.ShiftName,
                        ShiftPhase = shiftPhase,
                        IsCheckedIn = true,
                        IsShiftOpened = true
                    };
                }
                else
                {
                    return new CashierEligibilityResult
                    {
                        IsEligible = false,
                        ReasonCode = "OtherCashierActive",
                        Message = $"Hiện tại ca làm việc đang được phụ trách bởi thu ngân khác ({activeHandover.OutgoingCashier?.FullName}). Bạn chưa nhận bàn giao ca.",
                        ActiveShiftId = activeHandover.ShiftId,
                        ShiftName = matchingRoster.FixedShift.ShiftName,
                        ShiftPhase = shiftPhase,
                        IsCheckedIn = true,
                        IsShiftOpened = true
                    };
                }
            }

            var closedHandover = await _context.CashHandovers
                .AsNoTracking()
                .FirstOrDefaultAsync(ch => ch.BranchId == branchId
                                        && ch.ShiftId == matchingRoster.ShiftId
                                        && ch.HandoverDate.Date == matchingRoster.AssignmentDate.Date
                                        && ch.Status == CashHandoverConstants.ClosedStatus);

            if (closedHandover != null)
            {
                return new CashierEligibilityResult
                {
                    IsEligible = false,
                    ReasonCode = "ShiftClosed",
                    Message = $"Ca làm việc {matchingRoster.FixedShift.ShiftName} hôm nay đã được đóng/kết thúc. Quầy thu ngân đã khóa sổ.",
                    ActiveShiftId = matchingRoster.ShiftId,
                    ShiftName = matchingRoster.FixedShift.ShiftName,
                    ShiftPhase = shiftPhase,
                    IsCheckedIn = true,
                    IsShiftOpened = false
                };
            }

            if (!hasAnyHandoverToday)
            {
                return new CashierEligibilityResult
                {
                    IsEligible = false,
                    ReasonCode = "FirstShiftNotOpened",
                    Message = $"Ca làm việc chưa được Mở ca. Vui lòng thực hiện Mở ca trước khi bán hàng.",
                    ActiveShiftId = matchingRoster.ShiftId,
                    ShiftName = matchingRoster.FixedShift.ShiftName,
                    ShiftPhase = CashHandoverConstants.HandoverTypeFirstShift,
                    IsCheckedIn = true,
                    IsShiftOpened = false
                };
            }
            else
            {
                return new CashierEligibilityResult
                {
                    IsEligible = false,
                    ReasonCode = "MidShiftNotHandedOver",
                    Message = $"Bạn chưa nhận Bàn giao ca ({matchingRoster.FixedShift.ShiftName}) từ ca trước. Vui lòng thực hiện Nhận bàn giao ca để bắt đầu bán hàng.",
                    ActiveShiftId = matchingRoster.ShiftId,
                    ShiftName = matchingRoster.FixedShift.ShiftName,
                    ShiftPhase = shiftPhase,
                    IsCheckedIn = true,
                    IsShiftOpened = false
                };
            }
        }
    }
}
