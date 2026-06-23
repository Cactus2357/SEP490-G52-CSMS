using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Attendance
{
    /// <summary>
    /// BẢNG CA LÀM VIỆC CỐ ĐỊNH
    /// </summary>
    [Table("fixed_shifts")]
    public class FixedShift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("shift_id")]
        public int ShiftId { get; set; }

        /// <summary> Tên ca (Ca 1, Ca 2, Ca 3, Ca 4) </summary>
        [Required]
        [Column("shift_name")]
        [StringLength(50)]
        public string ShiftName { get; set; }

        /// <summary> Giờ bắt đầu ca </summary>
        [Required]
        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        /// <summary> Giờ kết thúc ca </summary>
        [Required]
        [Column("end_time")]
        public TimeSpan EndTime { get; set; }
    }
}
