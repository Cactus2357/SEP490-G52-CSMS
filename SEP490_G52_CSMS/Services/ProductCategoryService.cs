using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels.Sales.Category;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public ProductCategoryService(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<List<CategoryListViewModel>> GetCategoryListAsync(string? searchString)
        {
            var productCategories = await _productCategoryRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.Trim();

                productCategories = productCategories
                    .Where(c => !string.IsNullOrEmpty(c.CategoryName)
                        && c.CategoryName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return productCategories.Select(c => new CategoryListViewModel
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                Variants = string.Join(", ", c.MasterProducts
                    .SelectMany(p => p.ProductVariants)
                    .Select(v => v.SizeVariant)
                    .Distinct())
            }).ToList();
        }

        public async Task<bool> CreateCategoryAsync(CreateCategoryViewModel model)
        {
            var isNameExists = await _productCategoryRepository.IsCategoryNameExistsAsync(model.CategoryName);

            if (isNameExists)
            {
                return false;
            }

            var productCategory = new ProductCategory
            {
                CategoryName = model.CategoryName,
                Description = model.Description,
                Status = "Active"
            };

            await _productCategoryRepository.AddAsync(productCategory);

            return true;
        }

        public async Task<bool> UpdateCategoryAsync(UpdateCategoryViewModel model)
        {
            var productCategory = await _productCategoryRepository.GetByIdAsync(model.CategoryId);

            if (productCategory == null)
            {
                return false;
            }

            var isNameExists = await _productCategoryRepository.IsCategoryNameExistsAsync(model.CategoryName, model.CategoryId);

            if (isNameExists)
            {
                return false;
            }

            productCategory.CategoryName = model.CategoryName;
            productCategory.Description = model.Description;

            await _productCategoryRepository.UpdateAsync(productCategory);

            return true;
        }
    }
}