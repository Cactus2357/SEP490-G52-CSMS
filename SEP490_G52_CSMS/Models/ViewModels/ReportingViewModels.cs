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

        // For Chart
        public List<string> ChartLabels { get; set; } = new(); // Days in month
        public List<decimal> ChartData { get; set; } = new(); // Revenue values

        // For Table Data Grid
        public List<RevenueGridRow> TableData { get; set; } = new();
    }

    public class RevenueGridRow
    {
        public string BranchName { get; set; } = string.Empty;
        public decimal CashRevenue { get; set; }
        public decimal TransferRevenue { get; set; }
        public decimal TotalRevenue => CashRevenue + TransferRevenue;
    }

    public class ExportReportViewModel
    {
        // Global Dropdown
        public string? SelectedBranchId { get; set; }
        public List<SelectListItem> BranchList { get; set; } = new();
    }
}
