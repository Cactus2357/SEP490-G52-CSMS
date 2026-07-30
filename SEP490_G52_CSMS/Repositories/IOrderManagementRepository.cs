using System.Collections.Generic;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Repositories
{
    public interface IOrderManagementRepository
    {
        Task<List<Order>> GetOrdersAsync(string branchId, string searchCashier, string status, System.DateTime? fromDate = null, System.DateTime? toDate = null);
        Task<Order?> GetOrderDetailsAsync(string orderId, string branchId);
    }
}
