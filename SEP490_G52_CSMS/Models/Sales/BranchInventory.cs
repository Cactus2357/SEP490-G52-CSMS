using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SEP490_G52_CSMS.Models.Core;

namespace SEP490_G52_CSMS.Models.Sales
{
    [Table("branch_inventories")]
    public class BranchInventory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("inventory_id")]
        public int InventoryId { get; set; }

        [Required]
        [Column("branch_id")]
        [StringLength(20)]
        public string BranchId { get; set; } = string.Empty;

        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }

        [Required]
        [Column("material_id")]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public Material? Material { get; set; }

        [Required]
        [Column("stock_quantity")]
        public decimal StockQuantity { get; set; } = 0; // Tồn kho thực tế tại chi nhánh

        [Required]
        [Column("low_stock_threshold")]
        public decimal LowStockThreshold { get; set; } = 10; // Ngưỡng cảnh báo tồn kho thấp
    }
}
