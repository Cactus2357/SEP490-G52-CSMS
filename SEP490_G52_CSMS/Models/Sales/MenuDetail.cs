using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG CHI TIẾT SẢN PHẨM TRONG MENU (Map Many-to-Many)
    /// </summary>
    [Table("menu_details")]
    public class MenuDetail
    {
        [Column("menu_id")]
        public int MenuId { get; set; }

        [Column("variant_id")]
        public int VariantId { get; set; }

        [ForeignKey("MenuId")]
        public virtual BranchMenu? BranchMenu { get; set; }

        [ForeignKey("VariantId")]
        public virtual ProductVariant? ProductVariant { get; set; }
    }
}
