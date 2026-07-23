using System.Collections.Generic;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho trang lịch sử giao ca (có phân trang)
    /// </summary>
    public class HandoverHistoryViewModel
    {
        public List<HandoverHistoryItemViewModel> Items { get; set; } = new();
        public string BranchId { get; set; } = string.Empty;

        // Phân trang
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
