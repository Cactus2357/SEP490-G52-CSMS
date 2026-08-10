using System;
using System.Collections.Generic;
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
}
