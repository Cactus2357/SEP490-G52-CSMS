using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG BIẾN THỂ SẢN PHẨM - SIZE/GIÁ
    /// </summary>
    [Table("product_variants")]
    public class ProductVariant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("variant_id")]
        public int VariantId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("size_variant")]
        [StringLength(20)]
        public string SizeVariant { get; set; }

        [Required]
        [Column("selling_price")]
        public decimal SellingPrice { get; set; }

        [ForeignKey("ProductId")]
        public virtual MasterProduct MasterProduct { get; set; }
    }
}
