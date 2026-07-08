using System.Collections.Generic;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Repositories
{
    public interface IBranchRepository
    {
        Task<bool> BranchNameExistsAsync(string branchName);
        Task<string> GenerateNextBranchIdAsync();
        Task<List<Branch>> GetBranchesAsync(string? searchTerm, string? statusFilter, int? managerId, int pageIndex, int pageSize);
        Task<int> GetBranchCountAsync(string? searchTerm, string? statusFilter, int? managerId);
        Task<Branch?> GetBranchByIdAsync(string branchId);
        Task<List<Employee>> GetEligibleManagersAsync();
        Task AddBranchAsync(Branch branch);
        Task AddBranchManagerAsync(BranchManager assignment);
        Task UpdateBranchAsync(Branch branch);
        Task RemoveBranchManagersAsync(string branchId);
        Task DeleteBranchAsync(string branchId);
    }
}
