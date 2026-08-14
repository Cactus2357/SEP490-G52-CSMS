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
            // Tìm nhân viên có lịch trực tiếp theo trong ngày, tính từ thời gian hiện tại
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

        public async Task<CashHandover?> GetActiveHandoverByShiftAsync(string branchId, int shiftId, DateTime date)
        {
            return await _context.CashHandovers
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
