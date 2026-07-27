using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Repositories
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int employeeId);
        Task<Employee?> GetByUsernameAsync(string username);
        Task<bool> ExistsUsernameAsync(string username);
        Task<bool> ExistsEmailAsync(string email);
        Task<bool> ExistsCitizenIdAsync(string citizenId);
        Task<bool> ExistsPhoneNumberAsync(string phoneNumber);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task<IEnumerable<Branch>> GetActiveBranchesAsync();
        Task<bool> IsManagerOfBranchAsync(int managerId, string branchId);
        Task<bool> ExistsCitizenIdExcludeSelfAsync(string citizenId, int excludeEmployeeId);
        Task<List<Models.Attendance.WeeklyRosterGrid>> GetRostersWithAttendanceAndHandoverAsync(int employeeId);
        Task<bool> HasCashHandoverAsync(int employeeId, int shiftId, DateTime date);
    }
}
