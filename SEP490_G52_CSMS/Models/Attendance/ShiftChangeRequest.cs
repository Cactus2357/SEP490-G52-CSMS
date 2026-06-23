using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Attendance
{
    /// <summary>
    /// BẢNG YÊU CẦU ĐỔI CA LÀM VIỆC
    /// </summary>
    [Table("shift_change_requests")]
    public class ShiftChangeRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("request_id")]
        public int RequestId { get; set; }

        [Column("requesting_employee_id")]
        public int RequestingEmployeeId { get; set; }

        [Column("target_roster_id")]
        public int TargetRosterId { get; set; }

        [Column("substitute_employee_id")]
        public int SubstituteEmployeeId { get; set; }

        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Submitted";

        [ForeignKey("RequestingEmployeeId")]
        public virtual Employees.Employee RequestingEmployee { get; set; }

        [ForeignKey("TargetRosterId")]
        public virtual WeeklyRosterGrid TargetRoster { get; set; }

        [ForeignKey("SubstituteEmployeeId")]
        public virtual Employees.Employee SubstituteEmployee { get; set; }
    }
}
