namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class ShiftChangeListViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Status { get; set; } = "Tất cả";

        public List<ShiftChangeItemViewModel> Items { get; set; } = new();
    }

    public class ShiftChangeItemViewModel
    {
        public int RequestId { get; set; }
        public string SubmittedAt { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool CanCancel { get; set; }
    }

    public class ShiftChangeCreateViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty; // Mặc định từ giao diện chọn

        public string Aspiration { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
