using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG ĐƠN HÀNG GỐC - MASTER
    /// </summary>
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        [StringLength(50)]
        public string? OrderId { get; set; }

        [Column("branch_id")]
        public string? BranchId { get; set; }

        [Column("cashier_id")]
        public int CashierId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("payment_method")]
        [StringLength(200)]
        public string? PaymentMethod { get; set; }

        [Column("payment_status")]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Unpaid";

        [Column("brewing_status")]
        [StringLength(50)]
        public string BrewingStatus { get; set; } = "Waiting";

        [Column("recipient_name")]
        [StringLength(100)]
        public string? RecipientName { get; set; }

        /// <summary> Vị trí / Số bàn (VD: Bàn 05, Mang về) </summary>
        [Column("table_number")]
        [StringLength(50)]
        public string? TableNumber { get; set; }

        /// <summary> Tên khách hàng </summary>
        [Column("customer_name")]
        [StringLength(100)]
        public string? CustomerName { get; set; }

        /// <summary> Tiền tạm tính trước khi áp dụng giảm giá & chiết khấu </summary>
        [Column("subtotal_amount")]
        public decimal SubtotalAmount { get; set; } = 0;

        /// <summary> Số tiền giảm giá (voucher, chương trình khuyến mãi) </summary>
        [Column("discount_amount")]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary> Số tiền chiết khấu (chiết khấu % / khách quen) </summary>
        [Column("trade_discount_amount")]
        public decimal TradeDiscountAmount { get; set; } = 0;

        /// <summary> Ghi chú của thu ngân cho đơn hàng </summary>
        [Column("order_notes")]
        [StringLength(255)]
        public string? OrderNotes { get; set; }

        /// <summary> Mã voucher áp dụng cho đơn hàng </summary>
        [Column("voucher_id")]
        public int? VoucherId { get; set; }

        [Column("voucher_code")]
        [StringLength(50)]
        public string? VoucherCode { get; set; }

        [ForeignKey("VoucherId")]
        public virtual Voucher? Voucher { get; set; }

        [ForeignKey("BranchId")]
        public virtual Core.Branch? Branch { get; set; }

        [ForeignKey("CashierId")]
        public virtual Employees.Employee? Cashier { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();

        /// <summary> Danh sách các giao dịch thanh toán / hoàn tiền thuộc đơn hàng </summary>
        public virtual ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();

        // ==================== HELPER COMPUTED PROPERTIES ([NotMapped]) ====================
        /// <summary> Tổng số tiền thực thu thành công (cả tiền mặt & chuyển khoản) </summary>
        [NotMapped]
        public decimal PaidAmount => Payments?.Where(p => p.Status == "Success" && p.PaymentType == "Payment").Sum(p => p.Amount) ?? 0;

        /// <summary> Tổng tiền mặt đã thu </summary>
        [NotMapped]
        public decimal CashPaid => Payments?.Where(p => p.Status == "Success" && p.PaymentType == "Payment" && p.PaymentMethod == "Cash").Sum(p => p.Amount) ?? 0;

        /// <summary> Tổng tiền chuyển khoản đã thu </summary>
        [NotMapped]
        public decimal BankPaid => Payments?.Where(p => p.Status == "Success" && p.PaymentType == "Payment" && p.PaymentMethod != "Cash").Sum(p => p.Amount) ?? 0;

        /// <summary> Tổng số tiền đã hoàn trả cho khách </summary>
        [NotMapped]
        public decimal RefundedAmount => Payments?.Where(p => p.Status == "Success" && p.PaymentType == "Refund").Sum(p => p.Amount) ?? 0;

        /// <summary> Số tiền còn thiếu cần thanh toán </summary>
        [NotMapped]
        public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount);

        /// <summary> Mã giao dịch chuyển khoản mới nhất </summary>
        [NotMapped]
        public string? LatestBankTransactionCode => Payments?.OrderByDescending(p => p.CreatedAt).FirstOrDefault(p => !string.IsNullOrEmpty(p.TransactionCode))?.TransactionCode;

        /// <summary> Lý do hoàn tiền mới nhất </summary>
        [NotMapped]
        public string? LatestRefundReason => Payments?.OrderByDescending(p => p.CreatedAt).FirstOrDefault(p => p.PaymentType == "Refund" && !string.IsNullOrEmpty(p.Notes))?.Notes;

        /// <summary> Thời điểm hoàn tiền mới nhất </summary>
        [NotMapped]
        public DateTime? LatestRefundedAt => Payments?.OrderByDescending(p => p.CreatedAt).FirstOrDefault(p => p.PaymentType == "Refund")?.CreatedAt;

        // ==================== BACKWARD-COMPATIBLE READ-ONLY ALIASES ====================
        [NotMapped]
        public decimal CashAmount => CashPaid;

        [NotMapped]
        public decimal BankAmount => BankPaid;

        [NotMapped]
        public decimal RefundAmount => RefundedAmount;

        [NotMapped]
        public string? BankTransactionCode => LatestBankTransactionCode;

        [NotMapped]
        public string? RefundReason => LatestRefundReason;

        [NotMapped]
        public string? RefundMethod => Payments?.OrderByDescending(p => p.CreatedAt).FirstOrDefault(p => p.PaymentType == "Refund")?.PaymentMethod;

        [NotMapped]
        public DateTime? RefundedAt => LatestRefundedAt;
    }
}