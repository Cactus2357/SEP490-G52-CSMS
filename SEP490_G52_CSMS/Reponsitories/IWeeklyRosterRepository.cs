
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Repositories
{
    public interface IWeeklyRosterRepository
    {
        Task AddRangeAsync(IEnumerable<WeeklyRosterGrid> rosters);
        Task<bool> ExistsAsync(string branchId, DateTime assignmentDate, int shiftId);
        Task<List<FixedShift>> GetAllShiftsAsync();
        Task<List<Employee>> GetEmployeesByRoleAsync(string branchId, string role);
        Task<List<WeeklyRosterGrid>> GetRosterForWeekAsync(string branchId, DateTime weekStart, DateTime weekEnd);
        Task<List<WeeklyRosterGrid>> GetRosterForShiftAsync(string branchId, DateTime assignmentDate, int shiftId);
        void RemoveRange(IEnumerable<WeeklyRosterGrid> rosters);
        Task SaveAsync();
    }
}