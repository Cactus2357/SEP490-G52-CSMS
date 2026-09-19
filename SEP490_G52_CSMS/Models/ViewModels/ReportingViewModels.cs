using Microsoft.AspNetCore.Mvc.Rendering;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class RevenueReportViewModel
    {
        // For Filter
        public string? SelectedBranchId { get; set; }
        public string? SelectedMonth { get; set; } // Format: yyyy-MM
        public List<SelectListItem> BranchList { get; set; } = new();

        // Key Metrics Summary
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalMaterialImportsAmount { get; set; }

        // 1. Line Chart: Doanh thu theo thời gian
        public List<string> ChartLabels { get; set; } = new(); // Days in month
        public List<decimal> ChartData { get; set; } = new(); // Revenue values

        // 2. Table Data Grid: Doanh thu theo chi nhánh
        public List<RevenueGridRow> TableData { get; set; } = new();

        // 3. Doughnut Chart: Doanh thu & Số lượng theo danh mục sản phẩm
        public List<string> CategoryLabels { get; set; } = new();
        public List<decimal> CategoryRevenueData { get; set; } = new();
        public List<int> CategoryQuantityData { get; set; } = new();

        // 4. Bar Chart: Thống kê số lượng sản phẩm đã bán của từng sản phẩm
        public List<string> ProductSalesLabels { get; set; } = new();
        public List<int> ProductSalesQuantityData { get; set; } = new();

        // 5. Top sản phẩm bán chạy từng chi nhánh
        public List<BranchTopProductRow> BranchTopProducts { get; set; } = new();

        // 6. Bar Chart: Thống kê xuất nhập nguyên liệu (Kho tổng & Chi nhánh)
        public List<string> MaterialStatLabels { get; set; } = new();
        public List<decimal> WarehouseImportData { get; set; } = new();
        public List<decimal> WarehouseExportData { get; set; } = new();
        public List<decimal> BranchImportRequestData { get; set; } = new();
    }

    public class RevenueGridRow
    {
        public string BranchName { get; set; } = string.Empty;
        public decimal CashRevenue { get; set; }
        public decimal TransferRevenue { get; set; }
        public decimal TotalRevenue => CashRevenue + TransferRevenue;
        public int OrderCount { get; set; }
    }

    public class BranchTopProductRow
    {
        public string BranchName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ExportReportViewModel
    {
        public string? SelectedBranchId { get; set; }
        public List<SelectListItem> BranchList { get; set; } = new();
    }

    public class WarehouseReportViewModel
    {
        // Filters
        public string? SelectedBranchId { get; set; }
        public string? SelectedScope { get; set; } // all, warehouse, branch
        public string? SelectedStatus { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<SelectListItem> BranchList { get; set; } = new();
        public List<SelectListItem> StatusList { get; set; } = new();

        // Key Metrics Summary
        public decimal TotalImportCost { get; set; }
        public int TotalImportReceiptsCount { get; set; }
        public decimal TotalExportValue { get; set; }
        public int TotalExportRequestsCount { get; set; }
        public int CompletedRequestsCount { get; set; }
        public decimal TotalDefectiveQuantity { get; set; }
        public int DefectiveIncidentsCount { get; set; }

        // Charts
        public List<string> TrendLabels { get; set; } = new();
        public List<decimal> TrendImportData { get; set; } = new();
        public List<decimal> TrendExportData { get; set; } = new();

        public List<string> SupplierLabels { get; set; } = new();
        public List<decimal> SupplierData { get; set; } = new();

        public List<string> BranchExportLabels { get; set; } = new();
        public List<decimal> BranchExportData { get; set; } = new();

        public List<string> TopMaterialLabels { get; set; } = new();
        public List<decimal> TopMaterialData { get; set; } = new();

        // Tables / Tabs
        public List<WarehouseReceiptReportRow> Receipts { get; set; } = new();
        public List<BranchSupplyRequestReportRow> SupplyRequests { get; set; } = new();
        public List<SystemInventoryReportRow> Inventories { get; set; } = new();
        public List<DefectiveItemReportRow> DefectiveItems { get; set; } = new();
    }

    public class WarehouseReceiptReportRow
    {
        public int ReceiptId { get; set; }
        public string ReceiptCode { get; set; } = string.Empty;
        public DateTime ImportDate { get; set; }
        public string Supplier { get; set; } = string.Empty;
        public string DelivererName { get; set; } = string.Empty;
        public string? DelivererPhone { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ItemsCount => Items.Count;
        public List<ReceiptItemDetailDto> Items { get; set; } = new();
    }

    public class ReceiptItemDetailDto
    {
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string StorageUnit { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }

    public class BranchSupplyRequestReportRow
    {
        public int RequestId { get; set; }
        public string RequestCode { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? DelivererName { get; set; }
        public string? DelivererPhone { get; set; }
        public string? DeliveryProvider { get; set; }
        public string? ReceiverName { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string? InspectedBy { get; set; }
        public DateTime? InspectedAt { get; set; }
        public string? InspectionStatus { get; set; }
        public string? WarehouseNote { get; set; }
        public string? RequestNote { get; set; }
        public decimal TotalEstimatedValue { get; set; }
        public int ItemsCount => Items.Count;
        public List<SupplyRequestItemDetailDto> Items { get; set; } = new();
    }

    public class SupplyRequestItemDetailDto
    {
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string StorageUnit { get; set; } = string.Empty;
        public decimal QuantityRequested { get; set; }
        public decimal QuantityReleased { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal QuantityAccepted { get; set; }
        public decimal QuantityDefective { get; set; }
        public string? DefectType { get; set; }
        public string? DefectNote { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal EstimatedAmount => (QuantityReleased > 0 ? QuantityReleased : QuantityRequested) * UnitPrice;
    }

    public class SystemInventoryReportRow
    {
        public int MaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string MaterialKind { get; set; } = string.Empty;
        public string StorageUnit { get; set; } = string.Empty;
        public decimal StandardPrice { get; set; }
        public decimal CentralStockQuantity { get; set; }
        public decimal BranchStockQuantity { get; set; }
        public decimal TotalStockQuantity => CentralStockQuantity + BranchStockQuantity;
        public decimal LowStockThreshold { get; set; }
        public string StockStatus => TotalStockQuantity <= 0 ? "Hết hàng" : (TotalStockQuantity <= LowStockThreshold ? "Sắp hết" : "An toàn");
    }

    public class DefectiveItemReportRow
    {
        public string RequestCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public DateTime? InspectedAt { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string StorageUnit { get; set; } = string.Empty;
        public decimal QuantityDefective { get; set; }
        public string DefectType { get; set; } = string.Empty;
        public string? DefectNote { get; set; }
        public string? InspectedBy { get; set; }
        public string? DefectImageUrl { get; set; }
    }

    public class CashHandoverReportViewModel
    {
        // Filters
        public string? SelectedBranchId { get; set; }
        public string? SelectedHandoverType { get; set; }
        public string? SelectedDiscrepancyStatus { get; set; } // all, balanced, surplus, shortage
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<SelectListItem> BranchList { get; set; } = new();
        public List<SelectListItem> HandoverTypeList { get; set; } = new();

        // Key Metrics Summary
        public decimal TotalMachineCashRevenue { get; set; }
        public decimal TotalBankTransferRevenue { get; set; }
        public decimal TotalDepositedCash { get; set; }
        public decimal TotalDiscrepancy { get; set; }
        public int TotalHandoversCount { get; set; }
        public int DiscrepancyIncidentsCount { get; set; }

        // Grid Data
        public List<CashHandoverReportRow> Handovers { get; set; } = new();
    }

    public class CashHandoverReportRow
    {
        public int HandoverId { get; set; }
        public string BranchId { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public DateTime HandoverDate { get; set; }
        public int ShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public string ShiftTime { get; set; } = string.Empty;
        public string OutgoingCashierName { get; set; } = string.Empty;
        public string IncomingCashierName { get; set; } = string.Empty;
        public decimal InitialCash { get; set; }
        public decimal MachineCashRevenue { get; set; }
        public decimal BankTransferRevenue { get; set; }
        public decimal CashRefundAmount { get; set; }
        public decimal TheoreticalCash { get; set; }
        public decimal ActualCash { get; set; }
        public decimal DiscrepancyAmount => ActualCash - TheoreticalCash;
        public decimal RetainedCash { get; set; }
        public decimal DepositedCash { get; set; }
        public string HandoverType { get; set; } = string.Empty;
        public string? EmergencyReason { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
