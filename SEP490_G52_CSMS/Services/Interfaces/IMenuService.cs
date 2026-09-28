using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IMenuService
    {
        Task<List<MenuListViewModel>> GetMenusListAsync(string branchId);
        Task<MenuEditViewModel?> GetMenuForEditAsync(int menuId);
        Task ToggleMenuStatusAsync(string branchId, int menuId);
        Task<List<ProductSelectionViewModel>> GetMasterProductsForSelectionAsync(int? categoryId = null);
        Task<List<ProductCategory>> GetProductCategoriesAsync();
        Task AddMenuAsync(string branchId, string menuName, List<int> productIds);
        Task UpdateMenuProductsAsync(int menuId, List<int> productIds, string? menuName = null);
        Task<BartenderProductAvailabilityViewModel> GetBranchProductAvailabilityAsync(string branchId, string? search = null, int? categoryId = null, string? status = null);
        Task<(bool success, string message, bool newState)> ToggleProductAvailabilityAsync(string branchId, int productId, int updatedByEmployeeId);
        Task<(bool success, string message)> SetProductAvailabilityAsync(string branchId, int productId, bool isAvailable, int updatedByEmployeeId);
    }
}
