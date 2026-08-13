namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class OrderManagementListViewModel
    {
        public string BranchId { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string SearchCashier { get; set; } = string.Empty;
        public string FilterStatus { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? NextCursor { get; set; }
        public string? PrevCursor { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }

        public List<OrderManagementItemViewModel> Items { get; set; } = new List<OrderManagementItemViewModel>();
    }

    public class OrderManagementItemViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class OrderDetailViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string CashierName { get; set; } = string.Empty;
        public string ShiftInfo { get; set; } = string.Empty; // "Ca sáng - CN Q1"

        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;

        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }

        public List<OrderDetailItemViewModel> Items { get; set; } = new List<OrderDetailItemViewModel>();

        // Timeline (mocked if missing in DB)
        public string CreatedAtStr { get; set; } = string.Empty;
        public string PaidAtStr { get; set; } = string.Empty;
        public string CompletedAtStr { get; set; } = string.Empty;
    }

    public class OrderDetailItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; } // Quantity * UnitPrice
    }
}
