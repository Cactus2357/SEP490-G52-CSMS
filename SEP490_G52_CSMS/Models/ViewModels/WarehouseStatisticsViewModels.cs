using System;
using System.Collections.Generic;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class WarehouseStatisticsViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? SelectedRange { get; set; } = "month"; // "7days", "month", "quarter", "year", "custom"
        public string? SearchTerm { get; set; }
        public string? SelectedCategory { get; set; }
        public int? SelectedMaterialId { get; set; }

        // KPI Summary
        public decimal TotalImportQuantity { get; set; }
        public decimal TotalImportCost { get; set; }
        public decimal TotalExportQuantity { get; set; }
        public decimal TotalExportEstimatedValue { get; set; }
        public int TotalImportReceiptsCount { get; set; }
        public int TotalExportRequestsCount { get; set; }
        public int TotalMaterialsCount { get; set; }
        public int LowStockCount { get; set; }

        // Filters data
        public string? SelectedBranchId { get; set; }
        public List<SEP490_G52_CSMS.Models.Core.Branch> Branches { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<MaterialSelectOptionDto> MaterialOptions { get; set; } = new();

        // Table items
        public List<MaterialStatRowDto> MaterialStats { get; set; } = new();

        // Chart Data Transfer Objects
        public ChartComparisonDto BarChartData { get; set; } = new();
        public ChartTimelineDto LineChartData { get; set; } = new();
        public ChartRankDto HorizontalBarChartData { get; set; } = new();
    }

    public class MaterialSelectOptionDto
    {
        public int MaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class MaterialStatRowDto
    {
        public int MaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string StorageUnit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ImportQuantity { get; set; }
        public decimal ImportCost { get; set; }
        public decimal ExportQuantity { get; set; }
        public decimal ExportEstimatedValue { get; set; }
        public decimal NetChange => ImportQuantity - ExportQuantity;
    }

    public class ChartComparisonDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> ImportQuantities { get; set; } = new();
        public List<decimal> ExportQuantities { get; set; } = new();
    }

    public class ChartTimelineDto
    {
        public List<string> Dates { get; set; } = new();
        public List<decimal> ImportQuantities { get; set; } = new();
        public List<decimal> ExportQuantities { get; set; } = new();
    }

    public class ChartRankDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Quantities { get; set; } = new();
        public List<string> Units { get; set; } = new();
    }
}
