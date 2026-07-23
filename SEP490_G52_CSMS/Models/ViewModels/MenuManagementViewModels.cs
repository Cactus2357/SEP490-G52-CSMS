using System;
using System.Collections.Generic;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class MenuListViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class MenuEditViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public List<MenuProductViewModel> Products { get; set; } = new List<MenuProductViewModel>();
    }

    public class MenuProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SizeAndPrice { get; set; } = string.Empty;
    }

    public class ProductSelectionViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string SizeAndPrice { get; set; } = string.Empty;
    }
}
