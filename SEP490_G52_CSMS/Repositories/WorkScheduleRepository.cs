using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons.Constants;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Repositories.Interfaces;

namespace SEP490_G52_CSMS.Repositories
{
    public class WorkScheduleRepository : IWorkScheduleRepository
    {
        private readonly CSMSAppDbContext _context;

        public WorkScheduleRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetEmployeesByBranchAsync(string branchId)
        {
            return await _context.Employees
                .AsNoTracking()
                .Where(e => e.BranchId == branchId && e.Status == BranchConstants.DefaultStatus)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            return await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<List<FixedShift>> GetFixedShiftsAsync()
        {
            return await _context.FixedShifts
                .AsNoTracking()
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<WeeklyRosterGrid>> GetWeeklyRostersAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            return await _context.WeeklyRosterGrids
                .Include(w => w.FixedShift)
                .Include(w => w.AttendanceLogs)
                .AsNoTracking()
                .Where(w => w.EmployeeId == employeeId &&
                            w.AssignmentDate.Date >= startDate.Date &&
                            w.AssignmentDate.Date <= endDate.Date)
                .ToListAsync();
        }
    }
}
