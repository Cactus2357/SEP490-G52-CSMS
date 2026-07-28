using System.Collections.Generic;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services
{
    public interface IMenuService
    {
        Task<List<MenuListViewModel>> GetMenusListAsync(string branchId);
        Task<MenuEditViewModel?> GetMenuForEditAsync(int menuId);
        Task ToggleMenuStatusAsync(string branchId, int menuId);
        Task<List<ProductSelectionViewModel>> GetMasterProductsForSelectionAsync(int? categoryId = null);
        Task<List<ProductCategory>> GetProductCategoriesAsync();
        Task AddMenuAsync(string branchId, string menuName, List<int> productIds);
        Task UpdateMenuProductsAsync(int menuId, List<int> productIds);
    }
}
