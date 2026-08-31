using System.ComponentModel.DataAnnotations;
using SEP490_G52_CSMS.Commons;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class LeaveRequestListViewModel
    {
        public string BranchId { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        // Filters
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public List<LeaveRequestItemViewModel> Items { get; set; } = new List<LeaveRequestItemViewModel>();

        // Paging (if needed later)
        public int TotalCount { get; set; }
    }

    public class LeaveRequestItemViewModel
    {
        public int ApplicationId { get; set; }
        public string SubmittedAt { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string LeaveShifts { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool CanCancel => Status == "Pending" || Status == "Đã gửi" || Status == "Chờ duyệt";
    }

    public class LeaveRequestCreateViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui lòng nhập ca muốn nghỉ.")]
        public string LeaveShifts { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lý do.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ManagerLeaveRequestListViewModel
    {
        public string BranchId { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;

        // Filters
        public string SearchName { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Status { get; set; } = "Tất cả"; // "Tất cả", "Chờ duyệt", "Đã duyệt", "Từ chối"

        public int TotalCount => Items.Count;
        public List<ManagerLeaveRequestItemViewModel> Items { get; set; } = new();
    }

    public class ManagerLeaveRequestItemViewModel
    {
        public int ApplicationId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeFullName { get; set; } = string.Empty;
        public string EmployeeInitials { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public string RequestDateStr { get; set; } = string.Empty;
        public string FullDateStr { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Chờ duyệt", "Đã duyệt", "Từ chối"
        public string StatusBadgeClass { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty; // e.g. "Trần Quốc Bảo - 12/06 - Ca sáng - Bị ốm"
    }

    public class LeaveRequestDetailViewModel
    {
        public int ApplicationId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeFullName { get; set; } = string.Empty;
        public string EmployeeUsername { get; set; } = string.Empty;
        public string EmployeeRole { get; set; } = string.Empty;
        public string EmployeeInitials { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public string StatusBadgeClass { get; set; } = string.Empty;

        public string RequestDateStr { get; set; } = string.Empty;
        public string ShiftDetails { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string SubmittedAtStr { get; set; } = string.Empty;

        // Attendance stats in current month
        public int CurrentMonth { get; set; }
        public int TakenShiftsMonth { get; set; }
        public int PendingShiftsMonth { get; set; }
        public int TotalShiftsMonth { get; set; }

        // Shift warning banner
        public bool HasShiftWarning { get; set; }
        public string ShiftWarningMessage { get; set; } = string.Empty;

        public bool CanProcess => Status == "Chờ duyệt" || Status == "Pending" || Status == "Đã gửi";
    }
}
