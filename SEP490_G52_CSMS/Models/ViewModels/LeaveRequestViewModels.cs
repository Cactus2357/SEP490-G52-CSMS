using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public bool CanCancel => Status == "Pending" || Status == "Đã gửi";
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
}
