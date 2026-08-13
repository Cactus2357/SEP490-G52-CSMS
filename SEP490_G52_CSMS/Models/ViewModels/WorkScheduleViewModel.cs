namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class WorkScheduleViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;

        // Dải ngày hiển thị: VD "15/06/2026 - 21/06/2026"
        public string DateRangeText { get; set; } = string.Empty;
        public DateTime WeekStartDate { get; set; }

        // Danh sách các ngày trong tuần (Thứ 2 -> CN)
        public List<string> DayHeaders { get; set; } = new List<string>();

        // Ma trận ca làm việc: Row = Shift, Column = Day
        public List<WorkScheduleRowViewModel> Rows { get; set; } = new List<WorkScheduleRowViewModel>();
    }

    public class WorkScheduleRowViewModel
    {
        public int ShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public string ShiftTimeRange { get; set; } = string.Empty;

        // Danh sách các ô trong dòng tương ứng với các ngày trong tuần
        public List<WorkScheduleCellViewModel> Cells { get; set; } = new List<WorkScheduleCellViewModel>();
    }

    public class WorkScheduleCellViewModel
    {
        public DateTime Date { get; set; }
        public bool HasShift { get; set; }

        // Trạng thái điểm danh: "Absent", "NotYetCheckOut", "Present", "NotYetStarted"
        public string AttendanceStatus { get; set; } = "NotYetStarted";
    }

    public class SelectEmployeeScheduleViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
