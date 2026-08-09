using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SEP490_G52_CSMS.Models.Core;

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

        [Column("received_date")]
        public DateTime? ReceivedDate { get; set; } // Ngày nhận hàng (B Manager confirm)

        public List<BranchSupplyRequestItem> Items { get; set; } = new();
    }
}
