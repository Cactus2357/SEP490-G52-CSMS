using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEP490_G52_CSMS.Models.Attendance
{
    /// <summary>
    /// BẢNG BÀN GIAO KÉT TIỀN
    /// </summary>
    [Table("cash_handovers")]
    public class CashHandover
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("handover_id")]
        public int HandoverId { get; set; }

        [Column("branch_id")]
        public string BranchId { get; set; }

        [Column("handover_date")]
        public DateTime HandoverDate { get; set; } = DateTime.Today;

        [Column("shift_id")]
        public int ShiftId { get; set; }

        [Column("outgoing_cashier_id")]
        public int OutgoingCashierId { get; set; }

        [Column("incoming_cashier_id")]
        public int IncomingCashierId { get; set; }

        [Column("initial_cash")]
        public decimal InitialCash { get; set; }

        [Column("machine_cash_revenue")]
        public decimal MachineCashRevenue { get; set; }

        [Column("theoretical_cash")]
        public decimal TheoreticalCash { get; set; }

        [Column("actual_cash")]
        public decimal ActualCash { get; set; }

        [Column("notes")]
        public string? Notes { get; set; } = null;

        [Column("is_password_confirmed")]
        public bool IsPasswordConfirmed { get; set; } = false;

        [ForeignKey("BranchId")]
        public virtual Core.Branch? Branch { get; set; }

        [ForeignKey("ShiftId")]
        public virtual FixedShift? FixedShift { get; set; }

        [ForeignKey("OutgoingCashierId")]
        public virtual Employees.Employee? OutgoingCashier { get; set; }

        [ForeignKey("IncomingCashierId")]
        public virtual Employees.Employee? IncomingCashier { get; set; }
    }
}