using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Employees
{
    /// <summary>
    /// BẢNG PHÂN CÔNG QUẢN LÝ CHI NHÁNH (Khóa phức hợp)
    /// </summary>
    [Table("branch_managers")]
    public class BranchManager
    {
        [Column("branch_id")]
        public string BranchId { get; set; }

        [Column("manager_id")]
        public int ManagerId { get; set; }

        /// <summary> Ngày bổ nhiệm quản lý </summary>
        [Column("appointed_date")]
        public DateTime AppointedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("BranchId")]
        public virtual Core.Branch? Branch { get; set; }

        [ForeignKey("ManagerId")]
        public virtual Employee? Manager { get; set; }
    }
}
