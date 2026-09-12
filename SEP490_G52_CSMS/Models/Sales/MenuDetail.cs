using System.ComponentModel.DataAnnotations.Schema;
using SEP490_G52_CSMS.Models.Employees;

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

        [Column("is_available")]
        public bool IsAvailable { get; set; } = true;

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("MenuId")]
        public virtual BranchMenu? BranchMenu { get; set; }

        [ForeignKey("VariantId")]
        public virtual ProductVariant? ProductVariant { get; set; }

        [ForeignKey("UpdatedBy")]
        public virtual Employee? Updater { get; set; }
    }
}
