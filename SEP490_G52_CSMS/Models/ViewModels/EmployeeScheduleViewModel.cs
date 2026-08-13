using SEP490_G52_CSMS.Models.Attendance;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class EmployeeScheduleViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }

        public List<FixedShift> Shifts { get; set; } = new List<FixedShift>();
        public List<DateTime> Days { get; set; } = new List<DateTime>();

        // Dictionary to easily look up shift assignment and status by (ShiftId, Date)
        public Dictionary<(int ShiftId, DateTime Date), EmployeeShiftDetail> ScheduleData { get; set; } = new Dictionary<(int ShiftId, DateTime Date), EmployeeShiftDetail>();
    }

    public class EmployeeShiftDetail
    {
        public bool IsAssigned { get; set; }

        // E.g., "Present", "Absent", "NotYetCheckOut", or null if no log
        public string? OverallStatus { get; set; }

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }
}
