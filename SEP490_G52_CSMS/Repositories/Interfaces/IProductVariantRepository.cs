using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface IProductVariantRepository
    {
        Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId, string? searchString);
        Task<ProductVariant?> GetVariantByIdAsync(int variantId);
        Task<bool> IsVariantNameExistsAsync(int productId, string sizeVariant, int? excludeVariantId = null);
        Task AddVariantAsync(ProductVariant variant);
        Task UpdateVariantAsync(ProductVariant variant);
        Task DeleteVariantAsync(ProductVariant variant);
        Task<bool> CanDeleteVariantAsync(int variantId);
        Task<List<Material>> SearchMaterialsAsync(string term);
        Task<List<Recipe>> GetRecipeAsync(int variantId);
        Task SaveRecipeAsync(int variantId, List<Recipe> recipeItems);
    }
}
