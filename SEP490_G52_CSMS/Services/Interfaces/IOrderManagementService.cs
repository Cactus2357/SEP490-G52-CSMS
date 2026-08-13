using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IOrderManagementService
    {
        Task<OrderManagementListViewModel> GetOrderManagementListAsync(
            string branchId,
            string branchName,
            string searchCashier,
            string status,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? cursor = null,
            string direction = "next",
            int pageSize = 10);
        Task<OrderDetailViewModel?> GetOrderDetailAsync(string orderId, string branchId);
    }
}
