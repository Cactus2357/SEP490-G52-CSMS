using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Core
{
    /// <summary>
    /// BẢNG CHI NHÁNH
    /// </summary>
    [Table("branches")]
    public class Branch
    {
        /// <summary> Mã chi nhánh (Định dạng CB00X) </summary>
        [Key]
        [Column("branch_id")]
        [StringLength(20)]
        public string BranchId { get; set; }

        /// <summary> Tên chi nhánh (Duy nhất) </summary>
        [Required]
        [Column("branch_name")]
        [StringLength(255)]
        public string BranchName { get; set; }

        /// <summary> Địa chỉ chi nhánh </summary>
        [Required]
        [Column("address")]
        public string Address { get; set; }

        /// <summary> Số điện thoại chi nhánh </summary>
        [Column("phone_number")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary> Email chi nhánh </summary>
        [Column("email")]
        [StringLength(150)]
        public string? Email { get; set; }

        /// <summary> Giờ mở cửa </summary>
        [Required]
        [Column("opening_time")]
        public TimeSpan OpeningTime { get; set; }

        /// <summary> Giờ đóng cửa </summary>
        [Required]
        [Column("closing_time")]
        public TimeSpan ClosingTime { get; set; }

        /// <summary> Trạng thái hoạt động (Active, Inactive) </summary>
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        // Navigation Properties
        public virtual ICollection<Employees.Employee> Employees { get; set; } = new HashSet<Employees.Employee>();
        public virtual ICollection<Employees.BranchManager> BranchManagers { get; set; } = new HashSet<Employees.BranchManager>();
        public virtual ICollection<Attendance.WeeklyRosterGrid> WeeklyRosterGrids { get; set; } = new HashSet<Attendance.WeeklyRosterGrid>();
        public virtual ICollection<Attendance.CashHandover> CashHandovers { get; set; } = new HashSet<Attendance.CashHandover>();
        public virtual ICollection<Sales.BranchMenu> BranchMenus { get; set; } = new HashSet<Sales.BranchMenu>();
        public virtual ICollection<Sales.Order> Orders { get; set; } = new HashSet<Sales.Order>();
    }
}
