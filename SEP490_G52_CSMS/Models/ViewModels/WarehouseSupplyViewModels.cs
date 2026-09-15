using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class SupplyRequestSubmitModel
    {
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public string? RequestNote { get; set; }
        public List<SupplyRequestItemSubmitModel> Items { get; set; } = new();
    }

    public class SupplyRequestItemSubmitModel
    {
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class GoodsInspectionSubmitDto
    {
        public string RequestCode { get; set; } = string.Empty;
        public string? InspectorName { get; set; }
        public List<GoodsInspectionItemSubmitDto> Items { get; set; } = new();
    }

    public class GoodsInspectionItemSubmitDto
    {
        public int MaterialId { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal QuantityAccepted { get; set; }
        public decimal QuantityDefective { get; set; }
        public string? DefectType { get; set; }
        public string? DefectNote { get; set; }
        public List<IFormFile>? DefectImages { get; set; }
        public IFormFile? DefectImage { get; set; }
    }

    public class BranchSupplyRequestHistoryViewModel
    {
        public List<BranchSupplyRequest> Requests { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string SelectedStatus { get; set; } = "Tất cả";
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class BranchInventoryIndexViewModel
    {
        public List<BranchInventory> Items { get; set; } = new();
        public string? SearchString { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedKind { get; set; }
        public List<string> Categories { get; set; } = new();
        public List<string> Kinds { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class ExportRequestsViewModel
    {
        public List<BranchSupplyRequest> Requests { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string SelectedStatus { get; set; } = "Tất cả";
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class UpdateReleasedQuantitiesModel
    {
        public string RequestCode { get; set; } = string.Empty;
        public string? WarehouseNote { get; set; }
        public List<RequestItemReleasedModel> Items { get; set; } = new();
    }

    public class RequestItemReleasedModel
    {
        public int MaterialId { get; set; }
        public decimal QuantityReleased { get; set; }
    }

    public class MaterialSearchDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string MaterialKind { get; set; } = string.Empty;
        public string StorageUnit { get; set; } = string.Empty;
    }

    public class StockComparisonItemDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string StorageUnit { get; set; } = string.Empty;
        public decimal RequestedQuantity { get; set; }
        public decimal CurrentStockQuantity { get; set; }
        public decimal ReleasedQuantity { get; set; }
        public string Status { get; set; } = string.Empty; // "Đủ" hoặc "Thiếu"
    }

    public class BranchStockAdjustmentDto
    {
        public int? InventoryId { get; set; }
        public int? MaterialId { get; set; }
        public decimal ActualStock { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
