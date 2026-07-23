using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string recipientName, string branchId, int cashierId, List<OrderItem> items);
        Task<bool> ProcessPaymentAsync(string orderId, string paymentMethod);
        Task<bool> StartBrewingAsync(string orderId);
        Task<bool> CompleteBrewingAsync(string orderId);
        Task<OrderHistoryViewModel> GetOrderHistoryAsync(string status, DateTime? fromDate, DateTime? toDate, string search, int page = 1);
        Task<IEnumerable<OrderSummaryViewModel>> GetWaitingAndBrewingOrdersAsync();
        Task<OrderDetailViewModel?> GetOrderDetailsAsync(string orderId);
    }
}
