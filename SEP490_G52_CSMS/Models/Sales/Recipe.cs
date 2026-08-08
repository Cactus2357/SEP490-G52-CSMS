using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG CÔNG THỨC PHA CHẾ CHO TỪNG BIẾN THỂ SẢN PHẨM (UC36 Screen B)
    /// </summary>
    [Table("recipes")]
    public class Recipe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Required]
        [Column("variant_id")]
        public int VariantId { get; set; }

        [Required]
        [Column("material_id")]
        public int MaterialId { get; set; }

        [Required]
        [Column("quantity")]
        public decimal Quantity { get; set; } // Định lượng cần dùng theo đơn vị công thức (g hoặc ml)

        [Column("branch_id")]
        [StringLength(10)]
        public string? BranchId { get; set; } // Null nếu là công thức chung (Global), khác null nếu ghi đè theo chi nhánh

        [ForeignKey("VariantId")]
        public virtual ProductVariant? ProductVariant { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material? Material { get; set; }
    }
}
