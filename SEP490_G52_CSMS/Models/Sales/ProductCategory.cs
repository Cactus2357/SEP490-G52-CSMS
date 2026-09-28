using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG DANH MỤC SẢN PHẨM
    /// </summary>
    [Table("product_categories")]
    public class ProductCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [Column("category_name")]
        [StringLength(150)]
        public string? CategoryName { get; set; }
        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        public virtual ICollection<MasterProduct> MasterProducts { get; set; } = new HashSet<MasterProduct>();
    }
}
