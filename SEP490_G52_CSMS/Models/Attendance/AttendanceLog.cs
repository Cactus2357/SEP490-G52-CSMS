using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Attendance
{
    /// <summary>
    /// BẢNG CHẤM CÔNG CHI TIẾT CHECK-IN / CHECK-OUT
    /// </summary>
    [Table("attendance_logs")]
    public class AttendanceLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("attendance_id")]
        public int AttendanceId { get; set; }

        [Column("roster_id")]
        public int RosterId { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        // Check-in Data
        [Column("check_in_time")]
        public DateTime? CheckInTime { get; set; }

        [Column("is_face_check_in_valid")]
        public bool IsFaceCheckInValid { get; set; } = false;

        [Column("check_in_confidence")]
        public decimal? CheckInConfidence { get; set; }

        [Column("check_in_status")]
        [StringLength(50)]
        public string CheckInStatus { get; set; } = "Absent";

        // Check-out Data
        [Column("check_out_time")]
        public DateTime? CheckOutTime { get; set; }

        [Column("is_face_check_out_valid")]
        public bool IsFaceCheckOutValid { get; set; } = false;

        [Column("check_out_confidence")]
        public decimal? CheckOutConfidence { get; set; }

        [Column("check_out_status")]
        [StringLength(50)]
        public string CheckOutStatus { get; set; } = "NotYetCheckOut";

        [Column("overall_status")]
        [StringLength(50)]
        public string OverallStatus { get; set; } = "Absent";

        [Column("notes")]
        public string? Notes { get; set; }

        [ForeignKey("RosterId")]
        public virtual WeeklyRosterGrid? WeeklyRosterGrid { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees.Employee? Employee { get; set; }
    }
}