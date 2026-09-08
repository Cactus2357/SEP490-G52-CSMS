using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG GIAO DỊCH THANH TOÁN & HOÀN TIỀN (PAYMENT LEDGER)
    /// </summary>
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        [StringLength(50)]
        public string PaymentId { get; set; } = null!;

        [Column("order_id")]
        [StringLength(50)]
        public string OrderId { get; set; } = null!;

        [Column("branch_id")]
        [StringLength(20)]
        public string? BranchId { get; set; }

        [Column("cashier_id")]
        public int? CashierId { get; set; }

        /// <summary>
        /// Loại giao dịch: "Payment" (Thu tiền) | "Refund" (Hoàn tiền)
        /// </summary>
        [Column("payment_type")]
        [StringLength(20)]
        public string PaymentType { get; set; } = "Payment";

        /// <summary>
        /// Hình thức thanh toán: "Cash", "BankTransfer", "CreditCard", "Momo"
        /// </summary>
        [Column("payment_method")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash";

        /// <summary>
        /// Số tiền giao dịch (luôn là số dương)
        /// </summary>
        [Column("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Trạng thái giao dịch: "Success", "Pending", "Failed"
        /// </summary>
        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "Success";

        /// <summary>
        /// Mã tham chiếu / Mã GD ngân hàng / SePay ref code
        /// </summary>
        [Column("transaction_code")]
        [StringLength(100)]
        public string? TransactionCode { get; set; }

        /// <summary>
        /// Tiền khách đưa (nếu là tiền mặt)
        /// </summary>
        [Column("customer_cash")]
        public decimal? CustomerCash { get; set; }

        /// <summary>
        /// Tiền thừa trả lại khách (nếu là tiền mặt)
        /// </summary>
        [Column("change_amount")]
        public decimal? ChangeAmount { get; set; }

        /// <summary>
        /// Ghi chú giao dịch / Lý do hoàn tiền
        /// </summary>
        [Column("notes")]
        [StringLength(255)]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [ForeignKey("CashierId")]
        public virtual Employee? Cashier { get; set; }
    }
}
