using SEP490_G52_CSMS.Models.Attendance;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface ILeaveRequestRepository
    {
        Task<List<LeaveApplication>> GetLeaveRequestsAsync(int employeeId, DateTime? fromDate, DateTime? toDate, string status);
        Task<LeaveApplication?> GetLeaveRequestByIdAsync(int applicationId);
        Task AddLeaveRequestAsync(LeaveApplication leaveApplication);
        Task UpdateLeaveRequestAsync(LeaveApplication leaveApplication);

        // Branch Manager queries (UC47 & UC48)
        Task<List<LeaveApplication>> GetManagerLeaveRequestsAsync(string branchId, string? searchName, DateTime? fromDate, DateTime? toDate, string? status);
        Task<LeaveApplication?> GetLeaveRequestDetailByIdAsync(int applicationId);
        Task<int> GetApprovedLeavesCountInMonthAsync(int employeeId, int year, int month);
        Task<int> GetPendingLeavesCountInMonthAsync(int employeeId, int year, int month);
        Task<int> GetAssignedShiftsCountInMonthAsync(int employeeId, int year, int month);
        Task<int> GetOtherCoWorkersAssignedCountAsync(string branchId, DateTime date, int excludeEmployeeId);
        Task UnassignEmployeeRosterAsync(int employeeId, DateTime startDate, DateTime endDate);
    }
}
