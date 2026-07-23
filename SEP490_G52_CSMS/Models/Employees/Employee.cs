using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Employees
{
    /// <summary>
    /// BẢNG NHÂN VIÊN & XÁC THỰC
    /// </summary>
    [Table("employees")]
    public class Employee
    {
        /// <summary> Mã định danh nhân viên </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        /// <summary> Tên tài khoản (Viết liền không dấu) </summary>
        [Required]
        [Column("username")]
        [StringLength(100)]
        public string? Username { get; set; }

        /// <summary> Mật khẩu mã hóa </summary>
        [Required]
        [Column("password")]
        [StringLength(255)]
        public string? Password { get; set; }

        /// <summary> Họ và tên nhân viên </summary>
        [Required]
        [Column("full_name")]
        [StringLength(255)]
        public string? FullName { get; set; }

        /// <summary> Ngày sinh </summary>
        [Required]
        [Column("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        /// <summary> Địa chỉ thường trú </summary>
        [Required]
        [Column("address")]
        public string? Address { get; set; }

        /// <summary> Số điện thoại liên hệ </summary>
        [Required]
        [Column("phone_number")]
        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        /// <summary> Thư điện tử (Duy nhất) </summary>
        [Required]
        [Column("email")]
        [StringLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary> Vai trò (Cashier, Barista, Bartender, Busser, BranchManager) </summary>
        [Required]
        [Column("role")]
        [StringLength(50)]
        public string? Role { get; set; }

        /// <summary> Loại hình lao động (Full-time, Part-time) </summary>
        [Required]
        [Column("employment_type")]
        [StringLength(50)]
        public string? EmploymentType { get; set; }

        /// <summary> Số CCCD (Bắt buộc đúng 12 chữ số) </summary>
        [Required]
        [Column("citizen_id")]
        [StringLength(12), MinLength(12)]
        public string? CitizenId { get; set; }

        /// <summary> Đường dẫn lưu file hợp đồng (PDF/JPG) </summary>
        [Column("contract_file_path")]
        [StringLength(255)]
        public string? ContractFilePath { get; set; }

        /// <summary> Đường dẫn lưu file ảnh CCCD (PDF/JPG) </summary>
        [Column("cccd_file_path")]
        [StringLength(255)]
        public string? CccdFilePath { get; set; }

        /// <summary> Trạng thái tài khoản (Active, Inactive) </summary>
        [Column("status")]
        [StringLength(30)]
        public string Status { get; set; } = "Active";

        /// <summary> Vector đặc trưng khuôn mặt dùng để đối khớp AI </summary>
        [Column("face_data")]
        public string? FaceData { get; set; }

        /// <summary> Số lần thử đăng nhập thất bại liên tiếp (Tối đa 5) </summary>
        [Column("failed_login_attempts")]
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary> Thời hạn khóa tài khoản tạm thời </summary>
        [Column("lockout_until")]
        public DateTime? LockoutUntil { get; set; }

        /// <summary> Mã chi nhánh nhân viên thuộc về </summary>
        [Column("branch_id")]
        public string? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Core.Branch? Branch { get; set; }

        // Navigation Properties
        public virtual ICollection<BranchManager> ManagedBranches { get; set; } = new HashSet<BranchManager>();
        public virtual ICollection<Attendance.WeeklyRosterGrid> WeeklyRosterGrids { get; set; } = new HashSet<Attendance.WeeklyRosterGrid>();
        public virtual ICollection<Attendance.AttendanceLog> AttendanceLogs { get; set; } = new HashSet<Attendance.AttendanceLog>();
        public virtual ICollection<Attendance.LeaveApplication> LeaveApplications { get; set; } = new HashSet<Attendance.LeaveApplication>();
    }
}