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

        /// <summary> Mốc thời gian thực hiện hành động (Mở ca: OpenedAt / Bàn giao, Đóng ca: ClosedAt) </summary>
        public string ActionTime { get; set; } = string.Empty;
        public string ActionTimeType { get; set; } = string.Empty;

        /// <summary> Thông tin Người bàn giao (chỉ có khi Bàn giao ca hoặc Đóng ca) </summary>
        public string OutgoingCashierName { get; set; } = string.Empty;
        public bool HasOutgoing { get; set; } = true;

        /// <summary> Thông tin Người nhận ca (chỉ có khi Mở ca hoặc Bàn giao ca) </summary>
        public string IncomingCashierName { get; set; } = string.Empty;
        public bool HasIncoming { get; set; } = true;

        public string InitialCash { get; set; } = string.Empty;
        public string MachineCashRevenue { get; set; } = string.Empty;
        public string BankTransferRevenue { get; set; } = string.Empty;
        public string CashRefundAmount { get; set; } = string.Empty;
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
        /// <summary> Loại phiên: "Mở ca đầu ngày", "Bàn giao ca", "Bàn giao đột xuất", "Giao ca liên tiếp (Cùng NV)", "Đóng ca cuối ngày" </summary>
        public string SessionType { get; set; } = string.Empty;
        public string HandoverType { get; set; } = "Normal";
        public string? EmergencyReason { get; set; }
    }
}
