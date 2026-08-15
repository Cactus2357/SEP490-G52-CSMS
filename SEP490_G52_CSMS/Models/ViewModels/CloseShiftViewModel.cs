using System.ComponentModel.DataAnnotations;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Đóng Ca Cuối Ngày (Ca đêm / Chốt sổ ngày)
    /// </summary>
    public class CloseShiftViewModel
    {
        // ===== Hidden fields =====
        public int HandoverId { get; set; }
        public int OutgoingCashierId { get; set; }
        public string BranchId { get; set; } = string.Empty;

        // ===== Thông tin ca chốt ngày =====
        public string CashierName { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public string ShiftTimeRange { get; set; } = string.Empty;
        public DateTime HandoverDate { get; set; } = DateTime.Today;
        public DateTime OpenedAt { get; set; } = DateTime.Now;

        // ===== Doanh thu ca chốt ngày =====
        public decimal InitialCash { get; set; }
        public decimal MachineCashRevenue { get; set; }
        public decimal BankTransferRevenue { get; set; }
        public decimal CashRefundAmount { get; set; } = 0;

        /// <summary> Tổng tiền mặt lý thuyết két = InitialCash + MachineCashRevenue - CashRefundAmount </summary>
        public decimal TheoreticalCash => InitialCash + MachineCashRevenue - CashRefundAmount;

        // ===== Đối soát thực tế =====
        [Required(ErrorMessage = "Vui lòng nhập số tiền mặt thực tế trong két.")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền không hợp lệ.")]
        [Display(Name = "Tiền mặt thực tế")]
        public decimal ActualCash { get; set; }

        public decimal Discrepancy => ActualCash - TheoreticalCash;

        [StringLength(500)]
        [Display(Name = "Lý do chênh lệch (nếu có)")]
        public string? Notes { get; set; }

        /// <summary> Số tiền giữ lại trong két cho ca sáng mai </summary>
        [Display(Name = "Tiền để lại két cho ngày mai")]
        public decimal RetainedCashForTomorrow { get; set; } = 0;

        /// <summary> Số tiền nộp về két tổng / chủ quán </summary>
        [Display(Name = "Tiền nộp về két tổng / chủ quán")]
        public decimal DepositedCashAmount => Math.Max(0, ActualCash - RetainedCashForTomorrow);

        /// <summary> Thông tin người nhận tiền nộp cuối ngày (Quản lý / Chủ quán / Két tổng - bắt buộc) </summary>
        [Required(ErrorMessage = "Vui lòng nhập thông tin người nhận tiền.")]
        [Display(Name = "Thông tin người nhận tiền")]
        [StringLength(100, ErrorMessage = "Tên người nhận tiền không được vượt quá 100 ký tự.")]
        public string ReceiverName { get; set; } = string.Empty;

        // ===== Xác nhận đóng ca cuối ngày =====
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu xác nhận đóng ca cuối ngày.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu xác nhận")]
        public string Password { get; set; } = string.Empty;
    }
}
