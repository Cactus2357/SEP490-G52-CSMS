using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels.Sales.Product;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductService(
            IProductRepository productRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<ProductListViewModel>> GetProductListAsync(
            string? searchString)
        {
            var products = await _productRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = SEP490_G52_CSMS.Commons.StringHelper.RemoveDiacritics(searchString);

                products = products
                    .Where(p =>
                        !string.IsNullOrWhiteSpace(p.ProductName) &&
                        SEP490_G52_CSMS.Commons.StringHelper.RemoveDiacritics(p.ProductName).Contains(keyword))
                    .ToList();
            }

            return products.Select(p => new ProductListViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName ?? string.Empty,
                CategoryId = p.CategoryId,
                CategoryName = p.ProductCategory?.CategoryName
                               ?? string.Empty,
                ImageUrl = p.ImageUrl,
                Description = p.Description,
                Status = p.Status,
                Variants = string.Join(
                    ", ",
                    p.ProductVariants
                        .Select(v => v.SizeVariant)
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .Distinct())
            }).ToList();
        }

        public async Task<UpdateProductViewModel?>
            GetProductForUpdateAsync(int id)
        {
            var masterProduct =
                await _productRepository.GetByIdAsync(id);

            if (masterProduct == null)
            {
                return null;
            }

            return new UpdateProductViewModel
            {
                ProductId = masterProduct.ProductId,
                ProductName = masterProduct.ProductName
                              ?? string.Empty,
                CategoryId = masterProduct.CategoryId,
                Description = masterProduct.Description,
                ExistingImageUrl = masterProduct.ImageUrl,
                Status = masterProduct.Status
            };
        }

        public async Task<bool> CreateProductAsync(
            CreateProductViewModel model)
        {
            var productName = model.ProductName.Trim();

            var isNameExists =
                await _productRepository.IsProductNameExistsAsync(
                    productName);

            if (isNameExists)
            {
                return false;
            }

            string? imageUrl = null;

            if (model.ImageFile != null &&
                model.ImageFile.Length > 0)
            {
                imageUrl = await SaveImageAsync(
                    model.ImageFile);
            }

            var masterProduct = new MasterProduct
            {
                ProductName = productName,
                CategoryId = model.CategoryId,
                ImageUrl = imageUrl,
                Description = model.Description?.Trim(),
                Status = model.Status
            };

            await _productRepository.AddAsync(masterProduct);

            return true;
        }

        public async Task<bool> UpdateProductAsync(
            UpdateProductViewModel model)
        {
            var masterProduct =
                await _productRepository.GetByIdAsync(
                    model.ProductId);

            if (masterProduct == null)
            {
                return false;
            }

            var productName = model.ProductName.Trim();

            var isNameExists =
                await _productRepository.IsProductNameExistsAsync(
                    productName,
                    model.ProductId);

            if (isNameExists)
            {
                return false;
            }

            masterProduct.ProductName = productName;
            masterProduct.CategoryId = model.CategoryId;
            masterProduct.Description =
                model.Description?.Trim();
            masterProduct.Status = model.Status;

            if (model.ImageFile != null &&
                model.ImageFile.Length > 0)
            {
                masterProduct.ImageUrl =
                    await SaveImageAsync(model.ImageFile);
            }
            else
            {
                masterProduct.ImageUrl =
                    model.ExistingImageUrl;
            }

            await _productRepository.UpdateAsync(masterProduct);

            return true;
        }

        private async Task<string> SaveImageAsync(
            IFormFile imageFile)
        {
            const long maximumFileSize =
                5 * 1024 * 1024;

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (imageFile.Length > maximumFileSize)
            {
                throw new InvalidOperationException(
                    "Dung lượng hình ảnh không được vượt quá 5 MB.");
            }

            var extension = Path
                .GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Chỉ chấp nhận hình ảnh JPG, JPEG, PNG hoặc WEBP.");
            }

            var webRootPath =
                _webHostEnvironment.WebRootPath
                ?? Path.Combine(
                    _webHostEnvironment.ContentRootPath,
                    "wwwroot");

            var uploadFolder = Path.Combine(
                webRootPath,
                "uploads",
                "products");

            Directory.CreateDirectory(uploadFolder);

            var fileName =
                $"{Guid.NewGuid():N}{extension}";

            var filePath = Path.Combine(
                uploadFolder,
                fileName);

            await using var fileStream =
                new FileStream(
                    filePath,
                    FileMode.Create);

            await imageFile.CopyToAsync(fileStream);

            return $"/uploads/products/{fileName}";
        }
    }
}