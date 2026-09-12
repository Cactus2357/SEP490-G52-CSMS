using System.ComponentModel.DataAnnotations;
using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class VoucherIndexViewModel
    {
        public string BranchId { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string? StatusFilter { get; set; }
        public string? SearchTerm { get; set; }
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int ExpiredOrUsedUpCount { get; set; }
        public List<Voucher> Vouchers { get; set; } = new List<Voucher>();
    }

    public class CreateVoucherViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã voucher.")]
        [StringLength(50, ErrorMessage = "Mã voucher không được vượt quá 50 ký tự.")]
        [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Mã voucher chỉ được chứa chữ cái, số, dấu gạch ngang và gạch dưới.")]
        public string VoucherCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập % giảm giá.")]
        [Range(0.01, 100, ErrorMessage = "Mức giảm giá phải từ 0.01% đến 100%.")]
        public decimal DiscountPercent { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng voucher.")]
        [Range(1, 1000000, ErrorMessage = "Số lượng phát hành phải lớn hơn 0.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc.")]
        public DateTime EndDate { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }

    public class VoucherOrderItemViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string TableNumber { get; set; } = string.Empty;
        public decimal SubtotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
