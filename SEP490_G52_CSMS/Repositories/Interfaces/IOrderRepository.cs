using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(string orderId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task UpdateOrderAsync(Order order);
        Task<int> GetOrdersCountByDateAsync(DateTime date);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string paymentStatus, string brewingStatus);
    }
}
