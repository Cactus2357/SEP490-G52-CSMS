using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface IProductCategoryRepository
    {
        Task<List<ProductCategory>> GetAllAsync();

        Task<ProductCategory?> GetByIdAsync(int id);

        Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeCategoryId = null);

        Task AddAsync(ProductCategory productCategory);

        Task UpdateAsync(ProductCategory productCategory);
    }
}