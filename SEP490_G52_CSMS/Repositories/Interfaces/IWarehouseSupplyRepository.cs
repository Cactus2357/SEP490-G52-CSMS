using Microsoft.EntityFrameworkCore.Storage;
using SEP490_G52_CSMS.Models.Sales;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface IWarehouseSupplyRepository
    {
        Task<BranchSupplyRequest?> GetRequestByCodeAsync(string requestCode, string? branchId = null);
        Task<BranchSupplyRequest?> GetRequestByIdAsync(int requestId);
        Task<List<BranchSupplyRequest>> GetBranchRequestsAsync(string branchId, DateTime fromDate, DateTime toDate, string status, int page, int pageSize);
        Task<int> GetBranchRequestsCountAsync(string branchId, DateTime fromDate, DateTime toDate, string status);
        Task<List<BranchSupplyRequest>> GetCentralExportRequestsAsync(DateTime fromDate, DateTime toDate, string status, int page, int pageSize);
        Task<int> GetCentralExportRequestsCountAsync(DateTime fromDate, DateTime toDate, string status);
        Task<string> GenerateNextRequestCodeAsync();
        Task AddRequestAsync(BranchSupplyRequest request);
        Task UpdateRequestAsync(BranchSupplyRequest request);
        Task<List<BranchInventory>> GetBranchInventoriesAsync(string branchId);
        Task<BranchInventory?> GetBranchInventoryAsync(string branchId, int materialId);
        Task AddBranchInventoryAsync(BranchInventory inventory);
        Task<List<Material>> SearchTrackedMaterialsAsync(string branchId, string term);
        Task<Material?> GetMaterialByIdAsync(int materialId);
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
