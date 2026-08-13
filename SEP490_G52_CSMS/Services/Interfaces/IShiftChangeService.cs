using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IShiftChangeService
    {
        Task<ShiftChangeListViewModel> GetShiftChangeListAsync(int employeeId, string employeeName, string roleName, DateTime? fromDate, DateTime? toDate, string status);
        Task<OperationResult> CreateShiftChangeAsync(ShiftChangeCreateViewModel model);
        Task<OperationResult> CancelShiftChangeAsync(int requestId, int currentEmployeeId);
    }
}
