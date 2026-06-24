using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Attendance
{
    [Table("leave_applications")]
    public class LeaveApplication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("application_id")]
        public int ApplicationId { get; set; }

        [Required]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("reason")]
        public string? Reason { get; set; }

        [Required]
        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        // Tạo 2 cột để lưu khóa ngoại phức hợp trỏ sang bảng branch_managers
        [Column("approved_branch_id")]
        public string? ApprovedBranchId { get; set; }

        [Column("approved_manager_id")]
        public int? ApprovedManagerId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual Employees.Employee? Employee { get; set; }

        // Trỏ thực thể người duyệt về bảng BranchManager thay vì Employee thô
        public virtual Employees.BranchManager? Approver { get; set; }
    }
}