using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG SẢN PHẨM GỐC - MASTER
    /// </summary>
    [Table("master_products")]
    public class MasterProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("product_name")]
        [StringLength(255)]
        public string? ProductName { get; set; }

        [Column("image_url")]
        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        [ForeignKey("CategoryId")]
        public virtual ProductCategory? ProductCategory { get; set; }

        public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new HashSet<ProductVariant>();
    }
}
