using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Models.ViewModels.Sales.Category;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Controllers
{
    public class ProductCategoriesController : Controller
    {
        private readonly IProductCategoryService _productCategoryService;

        public ProductCategoriesController(IProductCategoryService productCategoryService)
        {
            _productCategoryService = productCategoryService;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var productCategories = await _productCategoryService.GetCategoryListAsync(searchString);

            ViewData["CurrentFilter"] = searchString;

            return View(productCategories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _productCategoryService.CreateCategoryAsync(model);

                if (!result)
                {
                    TempData["Error"] = "Lỗi trùng tên danh mục.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Thêm danh mục thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _productCategoryService.UpdateCategoryAsync(model);

                if (!result)
                {
                    TempData["Error"] = "Lỗi trùng tên danh mục.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Cập nhật danh mục thành công.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}