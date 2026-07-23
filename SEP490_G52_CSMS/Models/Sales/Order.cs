using System;
using System.Collections.Generic;
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
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("payment_method")]
        [StringLength(50)]
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

        [ForeignKey("BranchId")]
        public virtual Core.Branch? Branch { get; set; }

        [ForeignKey("CashierId")]
        public virtual Employees.Employee? Cashier { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    }
}