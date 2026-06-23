using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Attendance
{
    /// <summary>
    /// BẢNG ĐƠN XIN NGHỈ
    /// </summary>
    [Table("leave_applications")]
    public class LeaveApplication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("application_id")]
        public int ApplicationId { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("reason")]
        public string Reason { get; set; }

        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [Column("approved_by")]
        public int? ApprovedBy { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees.Employee Employee { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual Employees.Employee Approver { get; set; }
    }
}
