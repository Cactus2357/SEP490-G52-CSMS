using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IWarehouseSupplyService
    {
        // Branch Warehouse Operations
        Task<(bool Success, string Message, string? RequestCode)> CreateSupplyRequestAsync(string branchId, string branchName, SupplyRequestSubmitModel model, string createdBy);
        Task<(bool Success, string Message)> CancelSupplyRequestAsync(string requestCode, string branchId);
        Task<(bool Success, string Message, bool HasDefects)> InspectAndReceiveGoodsAsync(string branchId, string branchName, GoodsInspectionSubmitDto model, string currentUserName);
        Task<(bool Success, string Message, bool HasDefects)> ConfirmReceiptLegacyAsync(string requestCode, string branchId, string branchName, string delivererName, string delivererPhone);
        Task<List<BranchInventory>> GetBranchInventoryListAsync(string branchId);
        Task<List<MaterialSearchDto>> SearchTrackedMaterialsAsync(string branchId, string term);
        Task<BranchSupplyRequestHistoryViewModel> GetBranchRequestHistoryAsync(string branchId, DateTime fromDate, DateTime toDate, string status, int page, int pageSize);

        // Central Warehouse Operations
        Task<ExportRequestsViewModel> GetExportRequestsAsync(DateTime fromDate, DateTime toDate, string status, int page, int pageSize);
        Task<object?> GetRequestDetailsJsonAsync(string requestCode, string? branchId = null);
        Task<(bool Success, string Message)> ApproveAndPrepareShipmentAsync(string requestCode, string approverName);
        Task<(bool Success, string Message)> ConfirmShipmentAsync(string requestCode, string? delivererName, string? delivererPhone, string? deliveryProvider);
        Task<(bool Success, string Message)> RejectSupplyRequestAsync(string requestCode, string approverName, string? reason);
        Task<List<StockComparisonItemDto>> CompareStockAsync(string requestCode);
        Task<(bool Success, string Message)> UpdateReleasedQuantitiesAsync(string requestCode, List<RequestItemReleasedModel> items, string? warehouseNote);
    }
}
