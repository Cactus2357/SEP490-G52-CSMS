using SEP490_G52_CSMS.Models.ViewModels.Sales.Category;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IProductCategoryService
    {
        Task<List<CategoryListViewModel>> GetCategoryListAsync(string? searchString);

        Task<bool> CreateCategoryAsync(CreateCategoryViewModel model);

        Task<bool> UpdateCategoryAsync(UpdateCategoryViewModel model);
    }
}