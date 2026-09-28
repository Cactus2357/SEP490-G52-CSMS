using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Repositories.Interfaces;

namespace SEP490_G52_CSMS.Repositories
{
    public class WeeklyRosterRepository : IWeeklyRosterRepository
    {
        private readonly CSMSAppDbContext _context;

        public WeeklyRosterRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<WeeklyRosterGrid> rosters)
        {
            await _context.WeeklyRosterGrids.AddRangeAsync(rosters);
        }

        public async Task<bool> ExistsAsync(string branchId, DateTime assignmentDate, int shiftId)
        {
            return await _context.WeeklyRosterGrids.AnyAsync(x =>
                x.BranchId == branchId &&
                x.AssignmentDate.Date == assignmentDate.Date &&
                x.ShiftId == shiftId);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<List<FixedShift>> GetAllShiftsAsync()
        {
            return await _context.FixedShifts
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<Employee>> GetEmployeesByRoleAsync(string branchId, string role)
        {
            return await _context.Employees
                .Where(e => e.BranchId == branchId && e.Role == role && e.Status == "Active")
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<List<WeeklyRosterGrid>> GetRosterForWeekAsync(string branchId, DateTime weekStart, DateTime weekEnd)
        {
            return await _context.WeeklyRosterGrids
                .Include(w => w.Employee)
                .Include(w => w.AttendanceLogs)
                .Where(w => w.BranchId == branchId
                            && w.AssignmentDate.Date >= weekStart.Date
                            && w.AssignmentDate.Date <= weekEnd.Date)
                .ToListAsync();
        }

        public async Task<List<WeeklyRosterGrid>> GetRosterForShiftAsync(string branchId, DateTime assignmentDate, int shiftId)
        {
            return await _context.WeeklyRosterGrids
                .Include(w => w.Employee)
                .Include(w => w.FixedShift)
                .Include(w => w.AttendanceLogs)
                .Where(w => w.BranchId == branchId
                            && w.AssignmentDate.Date == assignmentDate.Date
                            && w.ShiftId == shiftId)
                .ToListAsync();
        }

        public void RemoveRange(IEnumerable<WeeklyRosterGrid> rosters)
        {
            _context.WeeklyRosterGrids.RemoveRange(rosters);
        }

        public void RemoveAttendanceLogs(IEnumerable<AttendanceLog> logs)
        {
            _context.AttendanceLogs.RemoveRange(logs);
        }
    }
}
