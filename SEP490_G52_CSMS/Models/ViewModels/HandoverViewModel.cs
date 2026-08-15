using System.ComponentModel.DataAnnotations;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Giao Ca & Đóng Ca
    /// </summary>
    public class HandoverViewModel
    {
        // ===== Hidden fields =====
        public int HandoverId { get; set; }
        public int OutgoingCashierId { get; set; }
        public string BranchId { get; set; } = string.Empty;

        // ===== Thông tin người bàn giao =====
        public string OutgoingCashierName { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;

        // ===== Doanh thu hệ thống tính =====
        /// <summary> Tiền mặt đầu ca (đã khai báo lúc mở ca) </summary>
        public decimal InitialCash { get; set; }

        /// <summary> Doanh thu CK (chuyển khoản) – lấy từ Orders </summary>
        public decimal BankTransferRevenue { get; set; }

        /// <summary> Doanh thu tiền mặt trên máy (POS) </summary>
        public decimal MachineCashRevenue { get; set; }

        /// <summary> Tiền mặt hoàn trả trong ca (do thiếu nguyên liệu / hủy món) </summary>
        public decimal CashRefundAmount { get; set; } = 0;

        /// <summary> Tổng tiền mặt lý thuyết phải có = InitialCash + MachineCashRevenue - CashRefundAmount </summary>
        public decimal TheoreticalCash => InitialCash + MachineCashRevenue - CashRefundAmount;

        /// <summary> Thời điểm mở ca </summary>
        public DateTime OpenedAt { get; set; } = DateTime.Now;

        /// <summary> Ca tiếp theo </summary>
        public string TargetShiftName { get; set; } = string.Empty;
        public string TargetShiftTimeRange { get; set; } = string.Empty;

        /// <summary> Cờ bàn giao cho chính mình (làm 2 ca liên tiếp) </summary>
        public bool IsSelfHandover { get; set; } = false;

        /// <summary> Cờ bàn giao khẩn cấp giữa ca (ốm/đột xuất) </summary>
        public bool IsEmergencyHandover { get; set; } = false;

        /// <summary> Lý do bàn giao đột xuất </summary>
        [Display(Name = "Lý do đột xuất")]
        public string? EmergencyReason { get; set; }

        // ===== Đối soát thực tế – Input người dùng =====
        /// <summary> Tiền thực tế đếm được trong két </summary>
        [Required(ErrorMessage = "Vui lòng nhập số tiền thực tế.")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền không hợp lệ.")]
        [Display(Name = "Tiền mặt thực tế")]
        public decimal ActualCash { get; set; }

        /// <summary> Chênh lệch = ActualCash - TheoreticalCash (hiển thị, không nhập) </summary>
        public decimal Discrepancy => ActualCash - TheoreticalCash;

        /// <summary> Lý do chênh lệch (tùy chọn) </summary>
        [StringLength(500)]
        [Display(Name = "Lý do (nếu có)")]
        public string? Notes { get; set; }

        /// <summary> Thông tin người giao tiền </summary>
        [StringLength(100)]
        [Display(Name = "Thông tin người giao tiền")]
        public string? DelivererName { get; set; }

        // ===== Đồng ý xác nhận – Input người dùng =====
        /// <summary> Danh sách thu ngân có thể nhận ca </summary>
        public List<CashierOption> IncomingCashiers { get; set; } = new();

        /// <summary> ID thu ngân được chọn để nhận ca </summary>
        [Required(ErrorMessage = "Vui lòng chọn người nhận ca.")]
        [Display(Name = "Nhân viên ca sau/xác nhận")]
        public int? IncomingCashierId { get; set; }

        /// <summary> Mật khẩu của người nhận ca để xác nhận </summary>
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu xác nhận.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhập mật khẩu")]
        public string IncomingPassword { get; set; } = string.Empty;
    }
}
