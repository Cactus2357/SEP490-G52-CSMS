using System.Threading.Tasks;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services
{
    public interface IOrderManagementService
    {
        Task<OrderManagementListViewModel> GetOrderManagementListAsync(string branchId, string branchName, string searchCashier, string status);
        Task<OrderDetailViewModel?> GetOrderDetailAsync(string orderId, string branchId);
    }
}
