using System;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services
{
    public interface ILeaveRequestService
    {
        Task<LeaveRequestListViewModel> GetLeaveRequestListAsync(int employeeId, string employeeName, DateTime? fromDate, DateTime? toDate, string status);
        Task<OperationResult> CreateLeaveRequestAsync(LeaveRequestCreateViewModel model);
        Task<OperationResult> CancelLeaveRequestAsync(int applicationId, int currentEmployeeId);
    }
}
