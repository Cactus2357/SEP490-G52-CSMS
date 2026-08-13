using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface IMenuRepository
    {
        Task<List<BranchMenu>> GetMenusByBranchAsync(string branchId);
        Task<BranchMenu?> GetMenuByIdAsync(int menuId);
        Task<BranchMenu> AddMenuAsync(BranchMenu menu);
        Task UpdateMenuAsync(BranchMenu menu);
        Task UpdateMenuProductsAsync(int menuId, List<int> variantIds);
        Task<List<MasterProduct>> GetMasterProductsAsync(int? categoryId = null);
        Task<List<ProductCategory>> GetProductCategoriesAsync();
        Task SetActiveMenuAsync(string branchId, int activeMenuId);
        Task<List<ProductVariant>> GetProductVariantsByProductIdAsync(int productId);
    }
}
