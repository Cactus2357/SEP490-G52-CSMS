using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string recipientName, string branchId, int cashierId, List<OrderItem> items);
        Task<bool> ProcessPaymentAsync(string orderId, string paymentMethod, decimal? customerCash = null, decimal? changeAmount = null, string? bankTransactionCode = null);
        Task<bool> ProcessSplitPaymentAsync(string orderId, decimal cashAmount, decimal bankAmount, decimal? customerCash = null, decimal? changeAmount = null, string? bankTransactionCode = null);
        Task<bool> ProcessRefundCashAsync(string orderId, decimal refundAmount, string reason, int cashierId);
        Task<bool> StartBrewingAsync(string orderId);
        Task<bool> CompleteBrewingAsync(string orderId);
        Task<bool> ReportMissingIngredientsAsync(string orderId, List<int> missingVariantIds, string? reason, int bartenderUserId);
        Task<(bool success, string message, decimal refundDifference, decimal additionalAmount)> ExchangeOrderItemsAsync(string orderId, List<OrderItemExchangeSubmission> newItems, string paymentMethod, decimal? customerCash, decimal? changeAmount, string? reason, int cashierId);
        Task<OrderHistoryViewModel> GetOrderHistoryAsync(string branchId, string status, DateTime? fromDate, DateTime? toDate, string search, int page = 1);
        Task<IEnumerable<OrderSummaryViewModel>> GetWaitingAndBrewingOrdersAsync(string? branchId = null);
        Task<SaleOrderDetailViewModel?> GetOrderDetailsAsync(string orderId);
    }
}
