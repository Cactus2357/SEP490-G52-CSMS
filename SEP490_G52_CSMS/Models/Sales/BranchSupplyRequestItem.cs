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

        [Column("quantity_received")]
        public decimal? QuantityReceived { get; set; } // Tổng SL thực nhận tại chi nhánh

        [Column("quantity_accepted")]
        public decimal? QuantityAccepted { get; set; } // SL đạt chuẩn nhập kho

        [Column("quantity_defective")]
        public decimal? QuantityDefective { get; set; } // SL lỗi / hư hỏng

        [Column("defect_type")]
        [StringLength(100)]
        public string? DefectType { get; set; } // Loại lỗi (Hết hạn, Hư hại do vận chuyển, Kém chất lượng, Giao sai/thiếu quy cách, Khác)

        [Column("defect_note")]
        [StringLength(500)]
        public string? DefectNote { get; set; } // Ghi chú chi tiết lỗi

        [Column("defect_image_url")]
        [StringLength(500)]
        public string? DefectImageUrl { get; set; } // Ảnh chụp bằng chứng
    }
}
