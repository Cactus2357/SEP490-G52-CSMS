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

        /// <summary> Mã giao dịch ngân hàng (đối soát chuyển khoản) </summary>
        [Column("bank_transaction_code")]
        [StringLength(100)]
        public string? BankTransactionCode { get; set; }

        /// <summary> Số tiền đã hoàn (nếu phát sinh hủy/thiếu nguyên liệu) </summary>
        [Column("refund_amount")]
        public decimal RefundAmount { get; set; } = 0;

        /// <summary> Lý do hoàn tiền </summary>
        [Column("refund_reason")]
        [StringLength(255)]
        public string? RefundReason { get; set; }

        /// <summary> Phương thức hoàn tiền: Cash / BankTransfer </summary>
        [Column("refund_method")]
        [StringLength(50)]
        public string? RefundMethod { get; set; }

        /// <summary> Thời điểm hoàn tiền </summary>
        [Column("refunded_at")]
        public DateTime? RefundedAt { get; set; }

        [ForeignKey("BranchId")]
        public virtual Core.Branch? Branch { get; set; }

        [ForeignKey("CashierId")]
        public virtual Employees.Employee? Cashier { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    }
}