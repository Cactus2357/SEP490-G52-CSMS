using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG QUẢN LÝ VOUCHER / MÃ GIẢM GIÁ
    /// </summary>
    [Table("vouchers")]
    public class Voucher
    {
        [Key]
        [Column("voucher_id")]
        public int VoucherId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("voucher_code")]
        public string VoucherCode { get; set; } = string.Empty;

        [Column("branch_id")]
        [StringLength(20)]
        public string? BranchId { get; set; }

        /// <summary> Tỷ lệ giảm giá % cho toàn bộ đơn hàng (1.00 đến 100.00) </summary>
        [Column("discount_percent", TypeName = "decimal(5, 2)")]
        public decimal DiscountPercent { get; set; }

        /// <summary> Tổng số lượng voucher phát hành </summary>
        [Column("quantity")]
        public int Quantity { get; set; }

        /// <summary> Số lượng voucher đã sử dụng </summary>
        [Column("used_count")]
        public int UsedCount { get; set; } = 0;

        /// <summary> Thời gian bắt đầu áp dụng </summary>
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        /// <summary> Thời gian kết thúc áp dụng </summary>
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        /// <summary> Thông tin thêm / mô tả chương trình giảm giá </summary>
        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary> Trạng thái hoạt động (1: Hoạt động, 0: Tạm dừng) </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual Employee? Creator { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();

        // ==================== HELPER PROPERTIES ====================
        /// <summary> Số lượng voucher còn lại có thể sử dụng </summary>
        [NotMapped]
        public int RemainingQuantity => Math.Max(0, Quantity - UsedCount);

        /// <summary> Kiểm tra voucher có đang trong thời gian hiệu lực và còn lượt dùng hay không </summary>
        [NotMapped]
        public bool IsValidNow => IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate && UsedCount < Quantity;

        /// <summary> Tình trạng văn bản hiển thị </summary>
        [NotMapped]
        public string StatusDisplay
        {
            get
            {
                if (!IsActive) return "Tạm dừng";
                var now = DateTime.Now;
                if (now < StartDate) return "Chưa bắt đầu";
                if (now > EndDate) return "Đã kết thúc";
                if (UsedCount >= Quantity) return "Đã hết lượt";
                return "Đang hoạt động";
            }
        }

        /// <summary> Thời lượng hoạt động của voucher </summary>
        [NotMapped]
        public string DurationDisplay
        {
            get
            {
                var duration = EndDate - StartDate;
                if (duration.TotalMinutes <= 0) return "0 phút";
                if (duration.TotalDays >= 1)
                {
                    int days = (int)duration.TotalDays;
                    int hours = duration.Hours;
                    return hours > 0 ? $"{days} ngày {hours} giờ" : $"{days} ngày";
                }
                int h = (int)duration.TotalHours;
                int m = duration.Minutes;
                return m > 0 ? $"{h} giờ {m} phút" : $"{h} giờ";
            }
        }
    }
}
