using SEP490_G52_CSMS.Models.ViewModels.Sales.Product;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductListViewModel>> GetProductListAsync(string? searchString);

        Task<UpdateProductViewModel?> GetProductForUpdateAsync(int id);

        Task<bool> CreateProductAsync(CreateProductViewModel model);

        Task<bool> UpdateProductAsync(UpdateProductViewModel model);

        Task<bool> ToggleProductStatusAsync(int productId);
    }
}