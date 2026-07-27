using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Mở Ca (UC11)
    /// </summary>
    public class OpenShiftViewModel
    {
        // ===== Hidden fields – truyền qua form =====
        public int CashierId { get; set; }
        public string BranchId { get; set; } = string.Empty;
        public int ShiftId { get; set; }

        // ===== Thông tin mở phiên làm việc =====
        /// <summary> Tên thu ngân thực hiện mở ca </summary>
        public string CashierName { get; set; } = string.Empty;

        /// <summary> Tên ca (Ca 1, Ca 2, ...) </summary>
        public string ShiftName { get; set; } = string.Empty;

        /// <summary> Giờ bắt đầu – kết thúc ca (VD: 06:00 – 10:00) </summary>
        public string ShiftTimeRange { get; set; } = string.Empty;

        /// <summary> Ngày thực hiện mở ca </summary>
        public DateTime HandoverDate { get; set; } = DateTime.Today;

        // ===== Thông tin ca trước (để đối chiếu) =====
        public string PreviousCashierName { get; set; } = "Chưa có dữ liệu";
        public string PreviousShiftName { get; set; } = "-";
        public string PreviousHandoverDate { get; set; } = "-";
        public string PreviousInitialCash { get; set; } = "-";
        public string PreviousApproverName { get; set; } = "-";

        // ===== Input người dùng =====
        /// <summary> Số tiền khai báo đầu ca (bắt buộc nhập) </summary>
        [Required(ErrorMessage = "Vui lòng nhập số tiền đầu ca.")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền không hợp lệ.")]
        [Display(Name = "Số tiền đang có trong két")]
        public decimal InitialCash { get; set; }

        // ===== Cờ điều khiển giao diện =====
        /// <summary> Cho biết đã đến giờ mở ca hay chưa </summary>
        public bool IsTimeToOpen { get; set; } = true;
    }
}
