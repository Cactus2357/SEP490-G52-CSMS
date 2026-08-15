using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons.Constants;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Repositories.Interfaces;

namespace SEP490_G52_CSMS.Repositories
{
    public class CashHandoverRepository : ICashHandoverRepository
    {
        private readonly CSMSAppDbContext _context;

        public CashHandoverRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<CashHandover?> GetActiveHandoverAsync(int cashierId, DateTime date)
        {
            return await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .Include(ch => ch.OutgoingCashier)
                .Include(ch => ch.IncomingCashier)
                .Include(ch => ch.Branch)
                .AsNoTracking()
                .FirstOrDefaultAsync(ch =>
                    ch.OutgoingCashierId == cashierId &&
                    ch.HandoverDate.Date == date.Date &&
                    ch.Status == CashHandoverConstants.ActiveStatus);
        }

        public async Task<CashHandover?> GetLastClosedHandoverForBranchAsync(string branchId)
        {
            return await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .Include(ch => ch.OutgoingCashier)
                .Include(ch => ch.IncomingCashier)
                .AsNoTracking()
                .Where(ch => ch.BranchId == branchId && ch.Status == CashHandoverConstants.ClosedStatus)
                .OrderByDescending(ch => ch.ClosedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<WeeklyRosterGrid?> GetCurrentRosterAsync(int cashierId, DateTime date)
        {
            return await _context.WeeklyRosterGrids
                .Include(w => w.FixedShift)
                .Include(w => w.Branch)
                .AsNoTracking()
                .Where(w =>
                    w.EmployeeId == cashierId &&
                    w.AssignmentDate.Date == date.Date)
                .OrderBy(w => w.FixedShift.StartTime)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetCurrentCashierIdAsync(string branchId, DateTime date, TimeSpan time)
        {
            var yesterday = date.Date.AddDays(-1);
            var rosters = await _context.WeeklyRosterGrids
                .Include(w => w.FixedShift)
                .AsNoTracking()
                .Where(w => w.BranchId == branchId && (w.AssignmentDate.Date == date.Date || w.AssignmentDate.Date == yesterday))
                .ToListAsync();

            var roster = rosters.FirstOrDefault(w => {
                if (w.FixedShift == null) return false;
                var start = w.FixedShift.StartTime;
                var end = w.FixedShift.EndTime;

                if (start <= end)
                {
                    return w.AssignmentDate.Date == date.Date && time >= start && time <= end;
                }
                else
                {
                    if (w.AssignmentDate.Date == date.Date) return time >= start;
                    else if (w.AssignmentDate.Date == yesterday) return time <= end;
                    return false;
                }
            });

            if (roster == null)
            {
                roster = rosters
                    .Where(w => w.AssignmentDate.Date == date.Date && w.FixedShift.StartTime > time)
                    .OrderBy(w => w.FixedShift.StartTime)
                    .FirstOrDefault();
            }

            if (roster == null)
            {
                roster = rosters
                    .Where(w => w.AssignmentDate.Date == date.Date)
                    .OrderByDescending(w => w.FixedShift.StartTime)
                    .FirstOrDefault();
            }

            return roster?.EmployeeId;
        }

        public async Task<int?> GetNextCashierForHandoverAsync(string branchId, DateTime date, TimeSpan currentTime)
        {
            // 1. Tìm ca tiếp theo dựa vào ca đang hoạt động
            var activeHandover = await GetCurrentActiveHandoverForBranchAsync(branchId, date);
            if (activeHandover != null)
            {
                var nextShift = await GetNextFixedShiftAsync(activeHandover.ShiftId);
                if (nextShift != null)
                {
                    var rosterForNextShift = await _context.WeeklyRosterGrids
                        .AsNoTracking()
                        .FirstOrDefaultAsync(w => w.BranchId == branchId && w.AssignmentDate.Date == date.Date && w.ShiftId == nextShift.ShiftId);

                    if (rosterForNextShift != null)
                    {
                        return rosterForNextShift.EmployeeId;
                    }
                }
            }

            // 2. Tìm nhân viên có ca trực tiếp theo trong ngày
            var roster = await _context.WeeklyRosterGrids
                .Include(w => w.FixedShift)
                .AsNoTracking()
                .Where(w => w.BranchId == branchId && w.AssignmentDate.Date == date.Date && w.FixedShift.StartTime >= currentTime)
                .OrderBy(w => w.FixedShift.StartTime)
                .FirstOrDefaultAsync();

            return roster?.EmployeeId;
        }

        public async Task<List<Employee>> GetCashiersInBranchAsync(string branchId)
        {
            return await _context.Employees
                .AsNoTracking()
                .Where(e =>
                    e.BranchId == branchId &&
                    e.Role == CashHandoverConstants.CashierRole &&
                    e.Status == BranchConstants.DefaultStatus)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<List<Employee>> GetEligibleHandoverCashiersAsync(string branchId, DateTime date, int currentShiftId, int outgoingCashierId)
        {
            var currentShift = await _context.FixedShifts
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ShiftId == currentShiftId);

            var currentStartTime = currentShift?.StartTime ?? TimeSpan.Zero;

            var rosterEmployeeIds = await _context.WeeklyRosterGrids
                .Include(r => r.FixedShift)
                .AsNoTracking()
                .Where(r => r.BranchId == branchId
                         && r.AssignmentDate.Date == date.Date
                         && r.FixedShift != null
                         && r.FixedShift.StartTime >= currentStartTime)
                .Select(r => r.EmployeeId)
                .Distinct()
                .ToListAsync();

            if (!rosterEmployeeIds.Contains(outgoingCashierId))
            {
                rosterEmployeeIds.Add(outgoingCashierId);
            }

            var eligibleCashiers = await _context.Employees
                .AsNoTracking()
                .Where(e => rosterEmployeeIds.Contains(e.EmployeeId) && e.Status == BranchConstants.DefaultStatus)
                .OrderBy(e => e.FullName)
                .ToListAsync();

            if (!eligibleCashiers.Any())
            {
                eligibleCashiers = await GetCashiersInBranchAsync(branchId);
            }

            return eligibleCashiers;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            return await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task AddHandoverAsync(CashHandover handover)
        {
            _context.CashHandovers.Add(handover);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateHandoverAsync(CashHandover handover)
        {
            _context.CashHandovers.Update(handover);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CashHandover>> GetHandoverHistoryAsync(string branchId, int pageIndex, int pageSize)
        {
            return await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .Include(ch => ch.OutgoingCashier)
                .Include(ch => ch.IncomingCashier)
                .AsNoTracking()
                .Where(ch => ch.BranchId == branchId)
                .OrderByDescending(ch => ch.OpenedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetHandoverCountAsync(string branchId)
        {
            return await _context.CashHandovers
                .CountAsync(ch => ch.BranchId == branchId);
        }

        public async Task<CashHandover?> GetHandoverByIdAsync(int handoverId)
        {
            return await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .Include(ch => ch.OutgoingCashier)
                .Include(ch => ch.IncomingCashier)
                .Include(ch => ch.Branch)
                .FirstOrDefaultAsync(ch => ch.HandoverId == handoverId);
        }

        public async Task<List<FixedShift>> GetAllFixedShiftsAsync()
        {
            return await _context.FixedShifts
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<string> GetShiftPhaseAsync(int shiftId)
        {
            var shifts = await GetAllFixedShiftsAsync();
            if (!shifts.Any()) return CashHandoverConstants.HandoverTypeFirstShift;

            if (shifts.First().ShiftId == shiftId)
            {
                return CashHandoverConstants.HandoverTypeFirstShift;
            }
            if (shifts.Last().ShiftId == shiftId)
            {
                return CashHandoverConstants.HandoverTypeLastShift;
            }
            return CashHandoverConstants.HandoverTypeMidShift;
        }

        public async Task<FixedShift?> GetNextFixedShiftAsync(int currentShiftId)
        {
            var shifts = await GetAllFixedShiftsAsync();
            var currentIndex = shifts.FindIndex(s => s.ShiftId == currentShiftId);
            if (currentIndex >= 0 && currentIndex < shifts.Count - 1)
            {
                return shifts[currentIndex + 1];
            }
            return null;
        }

        public async Task<(decimal cashRevenue, decimal bankRevenue, decimal cashRefunds)> GetShiftSalesStatsAsync(string branchId, int? cashierId, DateTime openedAt, DateTime? closedAt)
        {
            var query = _context.Orders
                .Where(o => o.BranchId == branchId && o.CreatedAt >= openedAt && o.PaymentStatus == "Paid");

            if (closedAt.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= closedAt.Value);
            }

            var orders = await query.ToListAsync();

            decimal cashRevenue = orders
                .Where(o => !string.IsNullOrEmpty(o.PaymentMethod) && o.PaymentMethod.StartsWith("Cash", StringComparison.OrdinalIgnoreCase))
                .Sum(o => o.TotalAmount);

            decimal bankRevenue = orders
                .Where(o => !string.IsNullOrEmpty(o.PaymentMethod) && !o.PaymentMethod.StartsWith("Cash", StringComparison.OrdinalIgnoreCase))
                .Sum(o => o.TotalAmount);

            decimal cashRefunds = orders
                .Where(o => o.RefundAmount > 0 && (string.IsNullOrEmpty(o.RefundMethod) || o.RefundMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase)))
                .Sum(o => o.RefundAmount);

            return (cashRevenue, bankRevenue, cashRefunds);
        }

        public async Task<(bool isEligible, string reasonCode, string message, int? activeShiftId, string? shiftPhase)> CheckCashierSaleEligibilityAsync(int cashierId, string branchId)
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var nowTime = DateTime.Now.TimeOfDay;

            // 1. Nếu nhân viên này ĐÃ CÓ ca làm việc đang Active tại chi nhánh hôm nay (đã mở ca hoặc đã nhận bàn giao ca sớm)
            var activeHandover = await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .AsNoTracking()
                .FirstOrDefaultAsync(ch => ch.BranchId == branchId 
                                        && (ch.OutgoingCashierId == cashierId || ch.IncomingCashierId == cashierId)
                                        && (ch.HandoverDate.Date == today || ch.HandoverDate.Date == yesterday) 
                                        && ch.Status == CashHandoverConstants.ActiveStatus);

            if (activeHandover != null && activeHandover.FixedShift != null)
            {
                var phase = await GetShiftPhaseAsync(activeHandover.ShiftId);
                return (true, "Eligible", "Hợp lệ", activeHandover.ShiftId, phase);
            }

            // 2. Tìm lịch phân công trực ca của nhân viên tại chi nhánh
            var rosters = await _context.WeeklyRosterGrids
                .Include(r => r.FixedShift)
                .Where(r => r.EmployeeId == cashierId && r.BranchId == branchId && (r.AssignmentDate.Date == today || r.AssignmentDate.Date == yesterday))
                .ToListAsync();

            if (!rosters.Any())
            {
                return (false, "NotScheduled", "Bạn không có lịch phân công ca làm việc tại chi nhánh hôm nay. Vui lòng kiểm tra lại Lịch làm việc.", null, null);
            }

            var matchingRoster = rosters.FirstOrDefault(r => {
                if (r.FixedShift == null) return false;
                var start = r.FixedShift.StartTime;
                var end = r.FixedShift.EndTime;
                var earlyWindow = TimeSpan.FromHours(2); // Cho phép mở/nhận ca sớm trước 2 tiếng

                if (start <= end)
                {
                    return r.AssignmentDate.Date == today && nowTime >= start.Subtract(earlyWindow) && nowTime <= end;
                }
                else
                {
                    if (r.AssignmentDate.Date == today) return nowTime >= start.Subtract(earlyWindow);
                    if (r.AssignmentDate.Date == yesterday) return nowTime <= end;
                    return false;
                }
            }) ?? rosters.OrderBy(r => r.FixedShift?.StartTime).FirstOrDefault();

            if (matchingRoster == null || matchingRoster.FixedShift == null)
            {
                return (false, "NotScheduled", "Bạn chưa có lịch phân công ca làm việc tại thời điểm hiện tại. Vui lòng kiểm tra lại Lịch làm việc.", null, null);
            }

            var shiftPhase = await GetShiftPhaseAsync(matchingRoster.ShiftId);

            if (shiftPhase == CashHandoverConstants.HandoverTypeFirstShift)
            {
                var shiftActive = await GetActiveHandoverByShiftAsync(branchId, matchingRoster.ShiftId, today);
                if (shiftActive == null)
                {
                    return (false, "FirstShiftNotOpened", $"Ca đầu ngày ({matchingRoster.FixedShift.ShiftName}) chưa được Mở ca. Vui lòng thực hiện Mở ca trước khi bán hàng.", matchingRoster.ShiftId, shiftPhase);
                }
                return (true, "Eligible", "Hợp lệ", matchingRoster.ShiftId, shiftPhase);
            }
            else
            {
                var shiftActive = await GetActiveHandoverByShiftAsync(branchId, matchingRoster.ShiftId, today);
                if (shiftActive == null)
                {
                    return (false, "MidShiftNotHandedOver", $"Bạn chưa nhận Bàn giao ca ({matchingRoster.FixedShift.ShiftName}) từ ca trước. Vui lòng thực hiện Nhận bàn giao ca để bắt đầu bán hàng.", matchingRoster.ShiftId, shiftPhase);
                }
                return (true, "Eligible", "Hợp lệ", matchingRoster.ShiftId, shiftPhase);
            }
        }

        public async Task<WeeklyRosterGrid?> GetRosterForShiftAsync(string branchId, int shiftId, DateTime date)
        {
            return await _context.WeeklyRosterGrids
                .Include(w => w.Employee)
                .Include(w => w.FixedShift)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.BranchId == branchId && w.ShiftId == shiftId && w.AssignmentDate.Date == date.Date);
        }

        public async Task<CashHandover?> GetCurrentActiveHandoverForBranchAsync(string branchId, DateTime date)
        {
            return await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .Include(ch => ch.OutgoingCashier)
                .Include(ch => ch.IncomingCashier)
                .Include(ch => ch.Branch)
                .AsNoTracking()
                .FirstOrDefaultAsync(ch => ch.BranchId == branchId && ch.HandoverDate.Date == date.Date && ch.Status == CashHandoverConstants.ActiveStatus);
        }

        public async Task<CashHandover?> GetActiveHandoverByShiftAsync(string branchId, int shiftId, DateTime date)
        {
            return await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .Include(ch => ch.OutgoingCashier)
                .Include(ch => ch.IncomingCashier)
                .AsNoTracking()
                .FirstOrDefaultAsync(ch => ch.BranchId == branchId
                                        && ch.ShiftId == shiftId
                                        && ch.HandoverDate.Date == date.Date
                                        && ch.Status == CashHandoverConstants.ActiveStatus);
        }

        public async Task SyncAttendanceOnOpenShiftAsync(int employeeId, int shiftId, DateTime date)
        {
            var roster = await _context.WeeklyRosterGrids
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.ShiftId == shiftId && w.AssignmentDate.Date == date.Date);

            if (roster != null)
            {
                var log = await _context.AttendanceLogs
                    .FirstOrDefaultAsync(a => a.RosterId == roster.RosterId && a.EmployeeId == employeeId);

                if (log == null)
                {
                    log = new AttendanceLog
                    {
                        RosterId = roster.RosterId,
                        EmployeeId = employeeId,
                        CheckInTime = DateTime.Now,
                        IsFaceCheckInValid = true,
                        CheckInStatus = "OnTime",
                        OverallStatus = "Present"
                    };
                    _context.AttendanceLogs.Add(log);
                }
                else
                {
                    if (log.CheckInTime == null) log.CheckInTime = DateTime.Now;
                    log.CheckInStatus = "OnTime";
                    log.OverallStatus = "Present";
                    _context.AttendanceLogs.Update(log);
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task SyncAttendanceOnCloseShiftAsync(int employeeId, int shiftId, DateTime date)
        {
            var roster = await _context.WeeklyRosterGrids
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.ShiftId == shiftId && w.AssignmentDate.Date == date.Date);

            if (roster != null)
            {
                var log = await _context.AttendanceLogs
                    .FirstOrDefaultAsync(a => a.RosterId == roster.RosterId && a.EmployeeId == employeeId);

                if (log != null)
                {
                    log.CheckOutTime = DateTime.Now;
                    log.CheckOutStatus = "CheckedOut";
                    _context.AttendanceLogs.Update(log);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
