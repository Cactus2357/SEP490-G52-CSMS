using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Services;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "RManager")]
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? statusFilter, int? managerId, int page = 1)
        {
            var model = await _branchService.GetBranchesAsync(searchTerm, statusFilter, managerId, page, 10);
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var model = await _branchService.GetBranchCreateModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchCreateViewModel model)
        {
            if (model.ClosingTime <= model.OpeningTime)
            {
                ModelState.AddModelError(nameof(model.ClosingTime), "Giờ đóng cửa phải sau giờ mở cửa.");
            }

            if (!ModelState.IsValid)
            {
                model.Managers = (await _branchService.GetBranchCreateModelAsync()).Managers;
                return View(model);
            }

            var result = await _branchService.CreateBranchAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.Managers = (await _branchService.GetBranchCreateModelAsync()).Managers;
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(string id)
        {
            var model = await _branchService.GetBranchDetailModelAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var model = await _branchService.GetBranchDetailModelAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string branchId)
        {
            var result = await _branchService.DeleteBranchAsync(branchId);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AssignManager(string id)
        {
            var model = await _branchService.GetBranchAssignManagerModelAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignManager(BranchAssignManagerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Managers = (await _branchService.GetBranchCreateModelAsync()).Managers;
                return View(model);
            }

            var result = await _branchService.AssignBranchManagerAsync(model);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = model.BranchId });
        }

        public async Task<IActionResult> Deactivate(string id)
        {
            var model = await _branchService.GetBranchDetailModelAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateConfirmed(string branchId)
        {
            var result = await _branchService.DeactivateBranchAsync(branchId);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = branchId });
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = branchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateConfirmed(string branchId)
        {
            var result = await _branchService.ActivateBranchAsync(branchId);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = branchId });
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = branchId });
        }

        public async Task<IActionResult> Edit(string id)
        {
            var model = await _branchService.GetBranchEditModelAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BranchEditViewModel model)
        {
            if (model.ClosingTime <= model.OpeningTime)
            {
                ModelState.AddModelError(nameof(model.ClosingTime), "Giờ đóng cửa phải sau giờ mở cửa.");
            }

            if (!ModelState.IsValid)
            {
                model.Managers = (await _branchService.GetBranchCreateModelAsync()).Managers;
                return View(model);
            }

            var result = await _branchService.UpdateBranchAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.Managers = (await _branchService.GetBranchCreateModelAsync()).Managers;
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
