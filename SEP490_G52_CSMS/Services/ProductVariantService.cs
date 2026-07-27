using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels.Sales.Product;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;

        public ProductVariantService(
            IProductVariantRepository variantRepository,
            IProductRepository productRepository)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
        }

        public async Task<VariantIndexViewModel?> GetVariantIndexAsync(int productId, string? searchString)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return null;
            }

            var variants = await _variantRepository.GetVariantsByProductIdAsync(productId, searchString);

            var model = new VariantIndexViewModel
            {
                ProductId = productId,
                ProductName = product.ProductName ?? string.Empty,
                SearchString = searchString,
                Variants = variants.Select(v => new VariantItemViewModel
                {
                    VariantId = v.VariantId,
                    SizeVariant = v.SizeVariant,
                    SellingPrice = v.SellingPrice
                }).ToList()
            };

            return model;
        }

        public async Task<(bool Success, string Message)> CreateVariantAsync(CreateVariantViewModel model)
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(model.ProductId);
            if (product == null)
            {
                return (false, "Không tìm thấy sản phẩm.");
            }

            // Validate name uniqueness
            if (string.IsNullOrWhiteSpace(model.SizeVariant))
            {
                 return (false, "Tên biến thể không được để trống.");
            }

            bool exists = await _variantRepository.IsVariantNameExistsAsync(model.ProductId, model.SizeVariant.Trim());
            if (exists)
            {
                return (false, "Tên biến thể đã tồn tại trong sản phẩm này.");
            }

            var newVariant = new ProductVariant
            {
                ProductId = model.ProductId,
                SizeVariant = model.SizeVariant.Trim(),
                SellingPrice = model.SellingPrice
            };

            await _variantRepository.AddVariantAsync(newVariant);
            return (true, "Thêm biến thể thành công.");
        }

        public async Task<UpdateVariantViewModel?> GetVariantForUpdateAsync(int variantId)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(variantId);
            if (variant == null)
            {
                return null;
            }

            return new UpdateVariantViewModel
            {
                VariantId = variant.VariantId,
                ProductId = variant.ProductId,
                SizeVariant = variant.SizeVariant,
                SellingPrice = variant.SellingPrice
            };
        }

        public async Task<(bool Success, string Message)> UpdateVariantAsync(UpdateVariantViewModel model)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(model.VariantId);
            if (variant == null)
            {
                return (false, "Không tìm thấy biến thể.");
            }

            if (string.IsNullOrWhiteSpace(model.SizeVariant))
            {
                 return (false, "Tên biến thể không được để trống.");
            }

            // Validate name uniqueness excluding itself
            bool exists = await _variantRepository.IsVariantNameExistsAsync(variant.ProductId, model.SizeVariant.Trim(), variant.VariantId);
            if (exists)
            {
                return (false, "Tên biến thể đã tồn tại trong sản phẩm này.");
            }

            variant.SizeVariant = model.SizeVariant.Trim();
            variant.SellingPrice = model.SellingPrice;

            await _variantRepository.UpdateVariantAsync(variant);
            return (true, "Cập nhật biến thể thành công.");
        }

        public async Task<(bool Success, string Message)> DeleteVariantAsync(int variantId)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(variantId);
            if (variant == null)
            {
                return (false, "Không tìm thấy biến thể.");
            }

            bool canDelete = await _variantRepository.CanDeleteVariantAsync(variantId);
            if (!canDelete)
            {
                return (false, "Không thể xóa biến thể này vì nó đang được sử dụng trong Đơn hàng hoặc Menu.");
            }

            await _variantRepository.DeleteVariantAsync(variant);
            return (true, "Xóa biến thể thành công.");
        }
    }
}
