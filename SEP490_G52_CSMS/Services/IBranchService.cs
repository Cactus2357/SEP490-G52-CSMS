using System.Threading.Tasks;
using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services
{
    public interface IBranchService
    {
        Task<BranchIndexViewModel> GetBranchesAsync(string? searchTerm, string? statusFilter, int? managerId, int pageIndex, int pageSize);
        Task<BranchCreateViewModel> GetBranchCreateModelAsync();
        Task<BranchEditViewModel?> GetBranchEditModelAsync(string branchId);
        Task<BranchDetailViewModel?> GetBranchDetailModelAsync(string branchId);
        Task<BranchAssignManagerViewModel?> GetBranchAssignManagerModelAsync(string branchId);
        Task<OperationResult> AssignBranchManagerAsync(BranchAssignManagerViewModel model);
        Task<OperationResult> DeactivateBranchAsync(string branchId);
        Task<OperationResult> CreateBranchAsync(BranchCreateViewModel model);
        Task<OperationResult> UpdateBranchAsync(BranchEditViewModel model);
        Task<OperationResult> DeleteBranchAsync(string branchId);
    }
}
