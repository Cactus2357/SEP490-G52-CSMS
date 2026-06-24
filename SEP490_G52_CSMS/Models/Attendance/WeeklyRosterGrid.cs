using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Attendance
{
    /// <summary>
    /// BẢNG PHÂN CÔNG LỊCH LÀM VIỆC HÀNG TUẦN
    /// </summary>
    [Table("weekly_roster_grids")]
    public class WeeklyRosterGrid
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("roster_id")]
        public int RosterId { get; set; }

        [Column("branch_id")]
        public string BranchId { get; set; }

        /// <summary> Ngày làm việc cụ thể trong tuần </summary>
        [Column("assignment_date")]
        public DateTime AssignmentDate { get; set; }

        [Column("shift_id")]
        public int ShiftId { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Core.Branch? Branch { get; set; }

        [ForeignKey("ShiftId")]
        public virtual FixedShift? FixedShift { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees.Employee? Employee { get; set; }
    }
}
