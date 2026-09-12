using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG CHI TIẾT CÁC MÓN TRONG ĐƠN HÀNG
    /// </summary>
    [Table("order_items")]
    public class OrderItem
    {
        [Column("order_id")]
        public string OrderId { get; set; }

        [Column("variant_id")]
        public int VariantId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("VariantId")]
        public virtual ProductVariant? ProductVariant { get; set; }
    }
}
