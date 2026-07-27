using SEP490_G52_CSMS.Models.ViewModels.Sales.Product;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IProductVariantService
    {
        Task<VariantIndexViewModel?> GetVariantIndexAsync(int productId, string? searchString);
        Task<(bool Success, string Message)> CreateVariantAsync(CreateVariantViewModel model);
        Task<UpdateVariantViewModel?> GetVariantForUpdateAsync(int variantId);
        Task<(bool Success, string Message)> UpdateVariantAsync(UpdateVariantViewModel model);
        Task<(bool Success, string Message)> DeleteVariantAsync(int variantId);
    }
}
