using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Sales
{
    [Table("branch_supply_request_items")]
    public class BranchSupplyRequestItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("request_item_id")]
        public int RequestItemId { get; set; }

        [Required]
        [Column("request_id")]
        public int RequestId { get; set; }

        [ForeignKey("RequestId")]
        public BranchSupplyRequest? BranchSupplyRequest { get; set; }

        [Required]
        [Column("material_id")]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public Material? Material { get; set; }

        [Required]
        [Column("quantity_requested")]
        public decimal QuantityRequested { get; set; } // Số lượng cần dùng

        [Column("quantity_released")]
        public decimal? QuantityReleased { get; set; } // Số lượng thực xuất
    }
}
