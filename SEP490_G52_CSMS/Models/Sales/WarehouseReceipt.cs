using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Sales
{
    [Table("warehouse_receipts")]
    public class WarehouseReceipt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("receipt_id")]
        public int ReceiptId { get; set; }

        [Required]
        [Column("receipt_code")]
        [StringLength(10)]
        public string ReceiptCode { get; set; } = string.Empty; // Mã phiếu (e.g. PN-001)

        [Required]
        [Column("import_date")]
        public DateTime ImportDate { get; set; } // Ngày nhập kho

        [Required]
        [Column("supplier")]
        [StringLength(100)]
        public string Supplier { get; set; } = string.Empty; // Nhà cung cấp

        [Required]
        [Column("total_amount")]
        public decimal TotalAmount { get; set; } // Tổng tiền phiếu nhập

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Đã nhập kho"; // Trạng thái (Đã nhập kho / Nháp)

        [Required]
        [Column("deliverer_name")]
        [StringLength(100)]
        public string DelivererName { get; set; } = string.Empty; // Họ & Tên Người giao

        [Column("deliverer_phone")]
        [StringLength(15)]
        public string? DelivererPhone { get; set; } // Số điện thoại người giao

        [Required]
        [Column("receiver_name")]
        [StringLength(100)]
        public string ReceiverName { get; set; } = string.Empty; // Người nhận hàng

        [Required]
        [Column("created_by")]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty; // Người lập phiếu (Quản Kho)

        public List<WarehouseReceiptItem> Items { get; set; } = new();
    }
}
