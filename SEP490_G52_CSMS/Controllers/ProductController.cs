using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SEP490_G52_CSMS.Models.ViewModels.Sales.Product;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "RManager")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IProductVariantService _variantService;

        public ProductController(
            IProductService productService,
            IProductCategoryService productCategoryService,
            IProductVariantService variantService)
        {
            _productService = productService;
            _productCategoryService = productCategoryService;
            _variantService = variantService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchString)
        {
            var products = await _productService
                .GetProductListAsync(searchString);

            var categories = await _productCategoryService
                .GetCategoryListAsync(null);

            var model = new ProductIndexViewModel
            {
                Products = products,
                SearchString = searchString,
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu sản phẩm không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _productService
                .CreateProductAsync(model);

            if (!result)
            {
                TempData["Error"] = "Lỗi trùng tên sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Thêm sản phẩm thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var model = await _productService
                .GetProductForUpdateAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            await LoadCategoriesAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            UpdateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(model);
                return View(model);
            }

            var result = await _productService
                .UpdateProductAsync(model);

            if (!result)
            {
                ModelState.AddModelError(
                    nameof(model.ProductName),
                    "Tên sản phẩm đã tồn tại hoặc dữ liệu không hợp lệ.");

                await LoadCategoriesAsync(model);

                return View(model);
            }

            TempData["Success"] = "Cập nhật sản phẩm thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _productService.ToggleProductStatusAsync(id);
            if (result)
            {
                TempData["Success"] = "Đã thay đổi trạng thái sản phẩm thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể cập nhật trạng thái sản phẩm.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesAsync(
            UpdateProductViewModel model)
        {
            var categories = await _productCategoryService
                .GetCategoryListAsync(null);

            model.Categories = categories.Select(c =>
                new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName,
                    Selected = c.CategoryId == model.CategoryId
                }).ToList();
        }

        // ==========================
        // VARIANT MANAGEMENT
        // ==========================

        [HttpGet]
        public async Task<IActionResult> Variants(int id, string? searchString)
        {
            var model = await _variantService.GetVariantIndexAsync(id, searchString);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVariant(CreateVariantViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu biến thể không hợp lệ.";
                return RedirectToAction(nameof(Variants), new { id = model.ProductId });
            }

            var result = await _variantService.CreateVariantAsync(model);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = result.Message;
            }

            return RedirectToAction(nameof(Variants), new { id = model.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVariant(UpdateVariantViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu biến thể không hợp lệ.";
                return RedirectToAction(nameof(Variants), new { id = model.ProductId });
            }

            var result = await _variantService.UpdateVariantAsync(model);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = result.Message;
            }

            return RedirectToAction(nameof(Variants), new { id = model.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(int variantId, int productId)
        {
            var result = await _variantService.DeleteVariantAsync(variantId);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = result.Message;
            }

            return RedirectToAction(nameof(Variants), new { id = productId });
        }

        [HttpGet]
        public async Task<IActionResult> SearchMaterials(string? term)
        {
            var materials = await _variantService.SearchMaterialsAsync(term ?? string.Empty);
            var result = materials.Select(m => new
            {
                materialId = m.MaterialId,
                materialCode = m.MaterialCode,
                materialName = m.MaterialName,
                materialKind = m.MaterialKind,
                category = m.Category,
                physicalState = m.PhysicalState,
                storageUnit = m.StorageUnit
            });
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecipe(int variantId)
        {
            var recipe = await _variantService.GetRecipeAsync(variantId);
            var result = recipe.Select(r => new
            {
                materialId = r.MaterialId,
                materialName = r.Material?.MaterialName ?? string.Empty,
                materialKind = r.Material?.MaterialKind ?? string.Empty,
                category = r.Material?.Category ?? string.Empty,
                quantity = r.Quantity,
                unit = r.Material?.PhysicalState == "Dạng lỏng" ? "ml" : "g"
            });
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRecipe([FromBody] SaveRecipeRequest request)
        {
            if (request == null || request.VariantId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var items = request.Items.Select(i => (i.MaterialId, i.Quantity)).ToList();
            var result = await _variantService.SaveRecipeAsync(request.VariantId, items);
            return Json(new { success = result.Success, message = result.Message });
        }
    }

    public class SaveRecipeRequest
    {
        public int VariantId { get; set; }
        public List<RecipeItemRequest> Items { get; set; } = new();
    }

    public class RecipeItemRequest
    {
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
    }
}