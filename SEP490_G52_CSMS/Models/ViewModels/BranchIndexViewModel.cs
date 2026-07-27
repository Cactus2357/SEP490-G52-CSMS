using System.Collections.Generic;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class BranchIndexViewModel
    {
        public List<BranchListItemViewModel> Branches { get; set; } = new List<BranchListItemViewModel>();
        public string? SearchTerm { get; set; }
        public string StatusFilter { get; set; } = "All";
        public int? ManagerId { get; set; }
        public List<string> StatusOptions { get; set; } = new List<string> { "All", "Active", "Inactive" };
        public List<BranchManagerOption> ManagerOptions { get; set; } = new List<BranchManagerOption>();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)System.Math.Ceiling((double)TotalCount / PageSize);
    }

    public class BranchManagerOption
    {
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        /// <summary>Tên chi nhánh đang quản lý (null nếu chưa quản lý chi nhánh nào)</summary>
        public string? CurrentBranchName { get; set; }
    }
}
