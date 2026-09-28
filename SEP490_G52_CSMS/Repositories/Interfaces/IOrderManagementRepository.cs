using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface IOrderManagementRepository
    {
        Task<List<Order>> GetOrdersAsync(
            string branchId,
            string searchCashier,
            string status,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            DateTime? cursor = null,
            string direction = "next",
            int pageSize = 10);
        Task<Order?> GetOrderDetailsAsync(string orderId, string branchId);
    }
}
