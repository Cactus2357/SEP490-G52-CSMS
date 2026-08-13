using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface IWorkScheduleRepository
    {
        Task<List<Employee>> GetEmployeesByBranchAsync(string branchId);
        Task<Employee?> GetEmployeeByIdAsync(int employeeId);
        Task<List<FixedShift>> GetFixedShiftsAsync();
        Task<List<WeeklyRosterGrid>> GetWeeklyRostersAsync(int employeeId, DateTime startDate, DateTime endDate);
    }
}
