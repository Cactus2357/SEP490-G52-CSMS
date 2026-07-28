using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SEP490_G52_CSMS.Controllers
{
    public class MenuManagementController : Controller
    {
        private readonly IMenuService _menuService;

        public MenuManagementController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // Dummy method to get branch ID for current manager
        private string GetCurrentBranchId()
        {
            // In a real application, you might get this from the logged-in user's claims or DB
            // We use a dummy branch "CB004" (Chi nhánh 1) for the mock-up
            return "CB004"; 
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var branchId = GetCurrentBranchId();
            var menus = await _menuService.GetMenusListAsync(branchId);
            return View(menus);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var branchId = GetCurrentBranchId();
            await _menuService.ToggleMenuStatusAsync(branchId, id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _menuService.GetMenuForEditAsync(id);
            if (model == null || model.BranchId != GetCurrentBranchId())
            {
                return NotFound();
            }

            ViewBag.Categories = await _menuService.GetProductCategoriesAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] System.Collections.Generic.List<int> SelectedProductIds)
        {
            var model = await _menuService.GetMenuForEditAsync(id);
            if (model == null || model.BranchId != GetCurrentBranchId())
            {
                return NotFound();
            }

            await _menuService.UpdateMenuProductsAsync(id, SelectedProductIds);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _menuService.GetProductCategoriesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string MenuName, [FromForm] System.Collections.Generic.List<int> SelectedProductIds)
        {
            if (string.IsNullOrWhiteSpace(MenuName))
            {
                ModelState.AddModelError("", "Tên thực đơn không được để trống.");
                ViewBag.Categories = await _menuService.GetProductCategoriesAsync();
                return View();
            }

            var branchId = GetCurrentBranchId();
            await _menuService.AddMenuAsync(branchId, MenuName, SelectedProductIds);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsPartial(int? categoryId)
        {
            var products = await _menuService.GetMasterProductsForSelectionAsync(categoryId);
            return PartialView("_ProductSelectionPartial", products);
        }
    }
}
