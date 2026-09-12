using SEP490_G52_CSMS.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Sales
{
    [Table("branch_supply_requests")]
    public class BranchSupplyRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("request_id")]
        public int RequestId { get; set; }

        [Required]
        [Column("request_code")]
        [StringLength(15)]
        public string RequestCode { get; set; } = string.Empty; // e.g., YC-001

        [Required]
        [Column("branch_id")]
        [StringLength(20)]
        public string BranchId { get; set; } = string.Empty;

        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }

        [Required]
        [Column("request_date")]
        public DateTime RequestDate { get; set; }

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Chờ duyệt"; // Chờ duyệt, Đã xuất kho, Đã hoàn thành, Từ chối, Đã hủy

        [Column("warehouse_note")]
        [StringLength(200)]
        public string? WarehouseNote { get; set; } // Ghi chú từ tổng kho

        [Column("approved_by")]
        [StringLength(100)]
        public string? ApprovedBy { get; set; } // Người duyệt đơn (Quản Kho)

        [Column("approved_date")]
        public DateTime? ApprovedDate { get; set; }

        [Column("deliverer_name")]
        [StringLength(100)]
        public string? DelivererName { get; set; } // Người giao hàng

        [Column("deliverer_phone")]
        [StringLength(15)]
        public string? DelivererPhone { get; set; } // SĐT người giao

        [Column("delivery_provider")]
        [StringLength(100)]
        public string? DeliveryProvider { get; set; } // Đơn vị vận chuyển / Biển số xe

        [Column("expected_delivery_date")]
        public DateTime? ExpectedDeliveryDate { get; set; } // Ngày mong muốn nhận hàng

        [Column("receiver_name")]
        [StringLength(100)]
        public string? ReceiverName { get; set; } // Người nhận hàng tại chi nhánh

        [Column("receiver_phone")]
        [StringLength(20)]
        public string? ReceiverPhone { get; set; } // SĐT người nhận

        [Column("request_note")]
        [StringLength(500)]
        public string? RequestNote { get; set; } // Ghi chú khi tạo yêu cầu

        [Column("received_date")]
        public DateTime? ReceivedDate { get; set; } // Ngày nhận hàng (B Manager confirm)

        [Column("inspected_by")]
        [StringLength(100)]
        public string? InspectedBy { get; set; } // Người đồng kiểm

        [Column("inspected_at")]
        public DateTime? InspectedAt { get; set; } // Thời gian đồng kiểm

        [Column("inspection_status")]
        [StringLength(50)]
        public string? InspectionStatus { get; set; } // Trạng thái đồng kiểm (Đạt 100%, Có hàng lỗi)

        public List<BranchSupplyRequestItem> Items { get; set; } = new();
    }
}
