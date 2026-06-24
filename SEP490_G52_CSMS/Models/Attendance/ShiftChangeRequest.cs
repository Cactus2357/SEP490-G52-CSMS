using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Attendance
{
    [Table("shift_change_requests")]
    public class ShiftChangeRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("request_id")]
        public int RequestId { get; set; }

        [Required]
        [Column("requesting_employee_id")]
        public int RequestingEmployeeId { get; set; }

        [Required]
        [Column("aspiration")]
        public string? Aspiration { get; set; }

        [Required]
        [Column("reason")]
        public string? Reason { get; set; }

        [Required]
        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Submitted";

        // Tạo 2 cột lưu khóa ngoại phức hợp trỏ sang bảng branch_managers
        [Column("approved_branch_id")]
        public string? ApprovedBranchId { get; set; }

        [Column("approved_manager_id")]
        public int? ApprovedManagerId { get; set; }


        [ForeignKey(nameof(RequestingEmployeeId))]
        public virtual Employees.Employee? RequestingEmployee { get; set; }

        // Trỏ thực thể người duyệt về bảng BranchManager
        public virtual Employees.BranchManager? Approver { get; set; }
    }
}