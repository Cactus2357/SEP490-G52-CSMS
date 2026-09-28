using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Sales
{
    [Table("warehouse_receipt_items")]
    public class WarehouseReceiptItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("receipt_item_id")]
        public int ReceiptItemId { get; set; }

        [Required]
        [Column("receipt_id")]
        public int ReceiptId { get; set; }

        [ForeignKey("ReceiptId")]
        public WarehouseReceipt? WarehouseReceipt { get; set; }

        [Required]
        [Column("material_id")]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public Material? Material { get; set; }

        [Required]
        [Column("quantity")]
        public decimal Quantity { get; set; } // Số lượng nhập kho

        [Required]
        [Column("unit_price")]
        public decimal UnitPrice { get; set; } // Đơn giá mua tương ứng đơn vị lưu kho

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; } // Thành tiền (Quantity * UnitPrice)
    }
}
