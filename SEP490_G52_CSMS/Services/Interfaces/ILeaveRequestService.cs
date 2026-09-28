using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<LeaveRequestListViewModel> GetLeaveRequestListAsync(int employeeId, string employeeName, DateTime? fromDate, DateTime? toDate, string status);
        Task<OperationResult> CreateLeaveRequestAsync(LeaveRequestCreateViewModel model);
        Task<OperationResult> CancelLeaveRequestAsync(int applicationId, int currentEmployeeId);

        // Branch Manager methods (UC47 & UC48)
        Task<ManagerLeaveRequestListViewModel> GetManagerLeaveRequestsAsync(string branchId, string branchName, string? searchName, DateTime? fromDate, DateTime? toDate, string status);
        Task<LeaveRequestDetailViewModel?> GetLeaveRequestDetailAsync(int applicationId, string branchId);
        Task<OperationResult> ApproveLeaveRequestAsync(int applicationId, string branchId, int managerId, string managerRole);
        Task<OperationResult> RejectLeaveRequestAsync(int applicationId, string branchId, int managerId, string managerRole, string? reason);
    }
}
