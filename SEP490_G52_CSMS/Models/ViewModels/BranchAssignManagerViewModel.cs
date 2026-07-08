using System.Collections.Generic;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class BranchAssignManagerViewModel
    {
        public string BranchId { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string CurrentManagerName { get; set; } = string.Empty;
        public string CurrentManagerCode { get; set; } = string.Empty;
        public string CurrentManagerAssignedDate { get; set; } = string.Empty;
        public int? SelectedManagerId { get; set; }
        public List<BranchManagerOption> Managers { get; set; } = new List<BranchManagerOption>();
    }
}
