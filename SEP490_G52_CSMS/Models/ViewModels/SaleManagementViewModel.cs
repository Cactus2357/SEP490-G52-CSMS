namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class CreateOrderViewModel
    {
        public IEnumerable<SaleCategoryViewModel> Categories { get; set; } = new List<SaleCategoryViewModel>();
        public IEnumerable<SaleProductViewModel> MenuProducts { get; set; } = new List<SaleProductViewModel>();
    }

    public class SaleCategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
    }

    public class SaleProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string ImageUrl { get; set; } = "";

        // The default variant (usually Size S)
        public int DefaultVariantId { get; set; }
        public decimal DefaultPrice { get; set; }

        // All variants for this product
        public List<SaleVariantViewModel> Variants { get; set; } = new List<SaleVariantViewModel>();
    }

    public class SaleVariantViewModel
    {
        public int VariantId { get; set; }
        public string SizeVariant { get; set; } = null!;
        public decimal SellingPrice { get; set; }
    }

    public class OrderHistoryViewModel
    {
        public string StatusFilter { get; set; } = "Tất cả";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchKeyword { get; set; } = "";

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;

        public List<OrderSummaryViewModel> Orders { get; set; } = new List<OrderSummaryViewModel>();
    }

    public class OrderSummaryViewModel
    {
        public string OrderId { get; set; } = null!;
        public string RecipientName { get; set; } = null!;
        public DateTime OrderTime { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string? BankTransactionCode { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string BrewingStatus { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal BankAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string? RefundReason { get; set; }
        public string DisplayStatus { get; set; } = null!;
        public bool IsMissingIngredients { get; set; }
        public decimal MissingItemsAmount { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    public class SaleOrderDetailViewModel
    {
        public string OrderId { get; set; } = null!;
        public string RecipientName { get; set; } = null!;
        public string? TableNumber { get; set; }
        public string? CustomerName { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TradeDiscountAmount { get; set; }
        public string? OrderNotes { get; set; }
        public string? BranchName { get; set; }
        public string? BranchAddress { get; set; }
        public string? BranchPhone { get; set; }
        public string? CashierName { get; set; }
        public DateTime OrderTime { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string? BankTransactionCode { get; set; }
        public string PaymentStatus { get; set; } = "";
        public string DisplayStatus { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal BankAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string? RefundReason { get; set; }
        public string? RefundMethod { get; set; }
        public DateTime? RefundedAt { get; set; }
        public bool IsMissingIngredients { get; set; }
        public decimal MissingItemsAmount { get; set; }
        public string? MissingIngredientsDetail { get; set; }

        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Size { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount => Quantity * UnitPrice;
        public bool IsMissing { get; set; }
    }

    /// <summary>
    /// Model gửi từ Bartender khi báo thiếu nguyên liệu cho một hoặc nhiều món
    /// </summary>
    public class MissingIngredientsReportModel
    {
        public string OrderId { get; set; } = string.Empty;
        public List<int> MissingVariantIds { get; set; } = new List<int>();
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Model gửi từ Cashier khi thực hiện đổi món / sửa đơn
    /// </summary>
    public class ExchangeOrderModel
    {
        public string OrderId { get; set; } = string.Empty;
        public List<OrderItemExchangeSubmission> Items { get; set; } = new List<OrderItemExchangeSubmission>();
        public string? AdditionalPaymentMethod { get; set; } = "Cash";
        public decimal? CustomerCash { get; set; }
        public decimal? ChangeAmount { get; set; }
        public string? Reason { get; set; }
    }

    public class OrderItemExchangeSubmission
    {
        public int VariantId { get; set; }
        public string? ProductName { get; set; }
        public string? Size { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
