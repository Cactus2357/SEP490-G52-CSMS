using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG THỰC ĐƠN CHI NHÁNH
    /// </summary>
    [Table("branch_menus")]
    public class BranchMenu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("menu_id")]
        public int MenuId { get; set; }

        [Column("branch_id")]
        public string? BranchId { get; set; }

        [Required]
        [Column("menu_name")]
        [StringLength(255)]
        public string? MenuName { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = false;

        [ForeignKey("BranchId")]
        public virtual Core.Branch? Branch { get; set; }

        public virtual ICollection<MenuDetail> MenuDetails { get; set; } = new HashSet<MenuDetail>();
    }
}
