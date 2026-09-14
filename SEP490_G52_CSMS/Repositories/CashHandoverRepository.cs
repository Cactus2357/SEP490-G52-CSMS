using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
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
            // Auto-close any leftover Active handovers from PAST days for this branch
            var today = DateTime.Today;
            var staleActiveHandovers = await _context.CashHandovers
                .Where(ch => ch.BranchId == branchId && ch.Status == CashHandoverConstants.ActiveStatus && ch.HandoverDate.Date < today.Date)
                .ToListAsync();

            if (staleActiveHandovers.Any())
            {
                foreach (var stale in staleActiveHandovers)
                {
                    stale.Status = CashHandoverConstants.ClosedStatus;
                    stale.ClosedAt = stale.OpenedAt.AddHours(8);
                    stale.ActualCash = stale.ActualCash > 0 ? stale.ActualCash : (stale.InitialCash + stale.MachineCashRevenue - stale.CashRefundAmount);
                    stale.Notes = (stale.Notes ?? "") + " [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]";
                }
                await _context.SaveChangesAsync();
            }

            return await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .Include(ch => ch.OutgoingCashier)
                .Include(ch => ch.IncomingCashier)
                .AsNoTracking()
                .Where(ch => ch.BranchId == branchId && ch.Status == CashHandoverConstants.ClosedStatus)
                .OrderByDescending(ch => ch.HandoverDate)
                .ThenByDescending(ch => ch.ClosedAt)
                .ThenByDescending(ch => ch.HandoverId)
                .FirstOrDefaultAsync();
        }

        public async Task<WeeklyRosterGrid?> GetCurrentRosterAsync(int cashierId, DateTime date)
        {
            var rosters = await _context.WeeklyRosterGrids
                .Include(w => w.FixedShift)
                .Include(w => w.Branch)
                .AsNoTracking()
                .Where(w =>
                    w.EmployeeId == cashierId &&
                    w.AssignmentDate.Date == date.Date)
                .ToListAsync();

            if (!rosters.Any()) return null;

            var nowTime = DateTime.Now.TimeOfDay;

            // 1. Match current shift bounds (start - 30m to end)
            var matching = rosters.FirstOrDefault(r => {
                if (r.FixedShift == null) return false;
                var start = r.FixedShift.StartTime;
                var end = r.FixedShift.EndTime;
                if (start <= end)
                {
                    return nowTime >= start.Subtract(TimeSpan.FromMinutes(30)) && nowTime <= end;
                }
                else
                {
                    return nowTime >= start.Subtract(TimeSpan.FromMinutes(30));
                }
            });

            // 2. If no strict time match, match roster where cashier is checked in
            if (matching == null)
            {
                var checkedInRosterIds = await _context.AttendanceLogs
                    .Where(a => a.EmployeeId == cashierId && a.CheckInTime != null && a.CheckOutTime == null)
                    .Select(a => a.RosterId)
                    .ToListAsync();

                matching = rosters.FirstOrDefault(r => checkedInRosterIds.Contains(r.RosterId));
            }

            // 3. Fallback: match upcoming shift or earliest shift
            if (matching == null)
            {
                matching = rosters.Where(r => r.FixedShift != null && r.FixedShift.StartTime >= nowTime)
                                  .OrderBy(r => r.FixedShift.StartTime)
                                  .FirstOrDefault()
                           ?? rosters.OrderBy(r => r.FixedShift?.StartTime).FirstOrDefault();
            }

            return matching;
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
                        .Include(w => w.Employee)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(w => w.BranchId == branchId 
                                               && w.AssignmentDate.Date == date.Date 
                                               && w.ShiftId == nextShift.ShiftId
                                               && w.Employee != null
                                               && (w.Employee.Role == CashHandoverConstants.CashierRole || w.Employee.Role == "Cashier")
                                               && w.Employee.Status == BranchConstants.DefaultStatus);

                    if (rosterForNextShift != null)
                    {
                        return rosterForNextShift.EmployeeId;
                    }
                }
            }

            // 2. Tìm thu ngân có ca trực tiếp theo trong ngày
            var roster = await _context.WeeklyRosterGrids
                .Include(w => w.FixedShift)
                .Include(w => w.Employee)
                .AsNoTracking()
                .Where(w => w.BranchId == branchId 
                         && w.AssignmentDate.Date == date.Date 
                         && w.FixedShift != null
                         && w.FixedShift.StartTime >= currentTime
                         && w.Employee != null
                         && (w.Employee.Role == CashHandoverConstants.CashierRole || w.Employee.Role == "Cashier")
                         && w.Employee.Status == BranchConstants.DefaultStatus)
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
                    (e.Role == CashHandoverConstants.CashierRole || e.Role == "Cashier") &&
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

            // Chỉ lấy nhân viên có vai trò Thu ngân (Cashier)
            var rosterEmployeeIds = await _context.WeeklyRosterGrids
                .Include(r => r.FixedShift)
                .Include(r => r.Employee)
                .AsNoTracking()
                .Where(r => r.BranchId == branchId
                         && r.AssignmentDate.Date == date.Date
                         && r.FixedShift != null
                         && r.FixedShift.StartTime >= currentStartTime
                         && r.Employee != null
                         && (r.Employee.Role == CashHandoverConstants.CashierRole || r.Employee.Role == "Cashier"))
                .Select(r => r.EmployeeId)
                .Distinct()
                .ToListAsync();

            if (!rosterEmployeeIds.Contains(outgoingCashierId))
            {
                rosterEmployeeIds.Add(outgoingCashierId);
            }

            var eligibleCashiers = await _context.Employees
                .AsNoTracking()
                .Where(e => rosterEmployeeIds.Contains(e.EmployeeId) 
                         && (e.Role == CashHandoverConstants.CashierRole || e.Role == "Cashier")
                         && e.Status == BranchConstants.DefaultStatus)
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

        public async Task<string> GetShiftPhaseAsync(int shiftId, string? branchId = null, DateTime? date = null)
        {
            var today = date ?? DateTime.Today;
            if (!string.IsNullOrEmpty(branchId))
            {
                bool hasHandoverToday = await _context.CashHandovers.AnyAsync(ch => ch.BranchId == branchId && ch.HandoverDate.Date == today.Date);
                if (!hasHandoverToday)
                {
                    return CashHandoverConstants.HandoverTypeFirstShift;
                }
            }

            var shifts = await GetAllFixedShiftsAsync();
            if (!shifts.Any()) return CashHandoverConstants.HandoverTypeLastShift;

            var next = await GetNextFixedShiftAsync(shiftId);
            if (next == null)
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
            DateTime openedLocal = openedAt.ToVietnamTime();
            DateTime? closedLocal = closedAt?.ToVietnamTime();

            var query = _context.Orders
                .Include(o => o.Payments)
                .Where(o => o.BranchId == branchId 
                         && (o.CreatedAt >= openedAt.AddSeconds(-30) || o.CreatedAt >= openedLocal.AddSeconds(-30))
                         && (o.PaymentStatus == "Paid" || o.PaymentStatus == "Partially Refunded" || o.PaymentStatus == "TransferSuccessPending"));

            if (closedAt.HasValue)
            {
                DateTime maxClosed = closedLocal.Value > closedAt.Value ? closedLocal.Value : closedAt.Value;
                query = query.Where(o => o.CreatedAt <= maxClosed.AddSeconds(60));
            }

            var orders = await query.ToListAsync();

            // MỌI ĐƠN HÀNG ĐÃ THANH TOÁN TRONG CA ĐỀU ĐƯỢC TÍNH VÀO DOANH THU CA (KHÔNG PHỤ THUỘC TRẠNG THÁI PHA CHẾ)
            var validPaidOrders = orders
                .Where(o => o.PaymentStatus != "Cancelled"
                         && o.PaymentStatus != "Refunded"
                         && o.BrewingStatus != "Cancelled / Refunded"
                         && !(o.RefundAmount > 0 && o.RefundAmount >= o.TotalAmount))
                .ToList();

            decimal cashRevenue = validPaidOrders
                .Sum(o => o.CashAmount > 0 
                    ? o.CashAmount 
                    : (!string.IsNullOrEmpty(o.PaymentMethod) && o.PaymentMethod.StartsWith("Cash", StringComparison.OrdinalIgnoreCase) ? o.TotalAmount : 0));

            decimal bankRevenue = validPaidOrders
                .Sum(o => o.BankAmount > 0 
                    ? o.BankAmount 
                    : (!string.IsNullOrEmpty(o.PaymentMethod) && !o.PaymentMethod.StartsWith("Cash", StringComparison.OrdinalIgnoreCase) ? o.TotalAmount : 0));

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

            // 0. KIỂM TRA KHUNG GIỜ BÁN HÀNG CỦA HỆ THỐNG (CHỈ TỪ 06:00 ĐẾN 24:00)
            if (nowTime < TimeSpan.FromHours(6))
            {
                return (false, "OutsideOperatingHours", "Hệ thống chỉ mở bán hàng từ 06:00 đến 24:00 hàng ngày. Hiện tại đang ngoài khung giờ phục vụ.", null, null);
            }

            // 1. KIỂM TRA LỊCH PHÂN CÔNG CA LÀM VIỆC CỦA THU NGÂN (ROSTER)
            var rosters = await _context.WeeklyRosterGrids
                .Include(r => r.FixedShift)
                .Include(r => r.AttendanceLogs)
                .Where(r => r.EmployeeId == cashierId && r.BranchId == branchId && (r.AssignmentDate.Date == today || r.AssignmentDate.Date == yesterday))
                .ToListAsync();

            if (!rosters.Any())
            {
                return (false, "NotScheduled", "Bạn không có lịch phân công ca làm việc tại chi nhánh hôm nay. Vui lòng kiểm tra lại Lịch làm việc.", null, null);
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
                return (false, "NotScheduled", "Hiện tại chưa tới giờ ca làm việc của bạn. Vui lòng kiểm tra lại Lịch làm việc.", null, null);
            }

            var shiftPhase = await GetShiftPhaseAsync(matchingRoster.ShiftId);

            // 2. KIỂM TRA ĐIỂM DANH CHẤM CÔNG (CHECK-IN HÀNG ĐẦU - ƯU TIÊN HƠN CASH HANDOVER)
            var attendanceLog = matchingRoster.AttendanceLogs.FirstOrDefault();
            bool isCheckedIn = (attendanceLog != null && attendanceLog.CheckInTime != null && (attendanceLog.OverallStatus == "Present" || attendanceLog.CheckInStatus == "OnTime" || attendanceLog.CheckInStatus == "Late"));

            if (!isCheckedIn)
            {
                return (false, "NotCheckedIn", $"Bạn chưa thực hiện điểm danh chấm công vào ca {matchingRoster.FixedShift.ShiftName}. Vui lòng điểm danh chấm công trước khi mở ca và bán hàng.", matchingRoster.ShiftId, shiftPhase);
            }

            if (attendanceLog!.CheckOutTime != null || attendanceLog.CheckOutStatus == "CheckedOut")
            {
                return (false, "ShiftEnded", $"Ca làm việc {matchingRoster.FixedShift.ShiftName} của bạn đã kết thúc (đã chấm công ra ca).", matchingRoster.ShiftId, shiftPhase);
            }

            // 3. KIỂM TRA ĐÃ CÓ PHIÊN CA ĐANG MỞ (ACTIVE) TẠI CHI NHÁNH HAY CHƯA
            var activeHandover = await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .Include(ch => ch.OutgoingCashier)
                .AsNoTracking()
                .FirstOrDefaultAsync(ch => ch.BranchId == branchId 
                                        && ch.ShiftId == matchingRoster.ShiftId
                                        && ch.HandoverDate.Date == matchingRoster.AssignmentDate.Date
                                        && ch.Status == CashHandoverConstants.ActiveStatus);

            if (activeHandover != null)
            {
                // Nếu ca này đang mở và đúng thu ngân này phụ trách -> Cho phép bán hàng ngay
                if (activeHandover.OutgoingCashierId == cashierId || activeHandover.IncomingCashierId == cashierId)
                {
                    return (true, "Eligible", "Hợp lệ", activeHandover.ShiftId, shiftPhase);
                }
                else
                {
                    return (false, "OtherCashierActive", $"Hiện tại ca làm việc đang được phụ trách bởi thu ngân khác ({activeHandover.OutgoingCashier?.FullName}). Bạn chưa nhận bàn giao ca.", activeHandover.ShiftId, shiftPhase);
                }
            }

            // 4. NẾU CHƯA CÓ CA ACTIVE: KIỂM TRA XEM CA NÀY ĐÃ BỊ ĐÓNG (CLOSED) HAY CHƯA
            var closedHandover = await _context.CashHandovers
                .AsNoTracking()
                .FirstOrDefaultAsync(ch => ch.BranchId == branchId
                                        && ch.ShiftId == matchingRoster.ShiftId
                                        && ch.HandoverDate.Date == matchingRoster.AssignmentDate.Date
                                        && ch.Status == CashHandoverConstants.ClosedStatus);

            if (closedHandover != null)
            {
                return (false, "ShiftClosed", $"Ca làm việc {matchingRoster.FixedShift.ShiftName} hôm nay đã được đóng/kết thúc. Quầy thu ngân đã khóa sổ.", matchingRoster.ShiftId, shiftPhase);
            }

            // 5. NẾU ĐÃ CHECK-IN NHƯNG CHƯA MỞ CA: HƯỚNG DẪN MỞ CA / NHẬN BÀN GIAO
            if (shiftPhase == CashHandoverConstants.HandoverTypeFirstShift)
            {
                return (false, "FirstShiftNotOpened", $"Ca đầu ngày ({matchingRoster.FixedShift.ShiftName}) chưa được Mở ca. Vui lòng thực hiện Mở ca trước khi bán hàng.", matchingRoster.ShiftId, shiftPhase);
            }
            else
            {
                return (false, "MidShiftNotHandedOver", $"Bạn chưa nhận Bàn giao ca ({matchingRoster.FixedShift.ShiftName}) từ ca trước. Vui lòng thực hiện Nhận bàn giao ca để bắt đầu bán hàng.", matchingRoster.ShiftId, shiftPhase);
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

        public async Task<bool> IsDayClosedAsync(string branchId, DateTime date)
        {
            return await _context.CashHandovers
                .AsNoTracking()
                .AnyAsync(ch => ch.BranchId == branchId
                             && ch.HandoverDate.Date == date.Date
                             && ch.HandoverType == CashHandoverConstants.HandoverTypeLastShift
                             && ch.Status == CashHandoverConstants.ClosedStatus);
        }

        public async Task<CashHandover?> GetClosedHandoverByShiftAsync(string branchId, int shiftId, DateTime date)
        {
            return await _context.CashHandovers
                .Include(ch => ch.FixedShift)
                .Include(ch => ch.OutgoingCashier)
                .Include(ch => ch.IncomingCashier)
                .AsNoTracking()
                .FirstOrDefaultAsync(ch => ch.BranchId == branchId
                                        && ch.ShiftId == shiftId
                                        && ch.HandoverDate.Date == date.Date
                                        && ch.Status == CashHandoverConstants.ClosedStatus);
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
                        CheckInTime = DateTime.UtcNow,
                        IsFaceCheckInValid = true,
                        CheckInStatus = "OnTime",
                        CheckOutTime = null,
                        CheckOutStatus = "NotYetCheckOut",
                        OverallStatus = "Present"
                    };
                    _context.AttendanceLogs.Add(log);
                }
                else
                {
                    if (log.CheckInTime == null) log.CheckInTime = DateTime.UtcNow;
                    log.CheckInStatus = "OnTime";
                    log.CheckOutTime = null;
                    log.CheckOutStatus = "NotYetCheckOut";
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
                    log.CheckOutTime = DateTime.UtcNow;
                    log.CheckOutStatus = "CheckedOut";
                    _context.AttendanceLogs.Update(log);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
