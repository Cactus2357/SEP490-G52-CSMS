namespace SEP490_G52_CSMS.Models.ViewModels
{
    /// <summary>
    /// Một dòng trong danh sách lịch sử giao ca
    /// </summary>
    public class HandoverHistoryItemViewModel
    {
        public int HandoverId { get; set; }
        public string HandoverDate { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public string OutgoingCashierName { get; set; } = string.Empty;
        public string IncomingCashierName { get; set; } = string.Empty;
        public string InitialCash { get; set; } = string.Empty;
        public string MachineCashRevenue { get; set; } = string.Empty;
        public string TheoreticalCash { get; set; } = string.Empty;
        public string ActualCash { get; set; } = string.Empty;
        public string Discrepancy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OpenedAt { get; set; } = string.Empty;
        public string ClosedAt { get; set; } = string.Empty;
        /// <summary> Lý do chênh lệch tiền (nếu có) </summary>
        public string? Notes { get; set; }
        /// <summary> Thông tin người giao tiền </summary>
        public string? DelivererName { get; set; }
        /// <summary> Loại phiên: "Mở ca" hoặc "Giao ca" </summary>
        public string SessionType { get; set; } = string.Empty;
    }
}
