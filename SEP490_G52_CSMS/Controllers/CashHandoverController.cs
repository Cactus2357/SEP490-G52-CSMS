using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize]
    public class CashHandoverController : Controller
    {
        private readonly ICashHandoverService _cashHandoverService;

        public CashHandoverController(ICashHandoverService cashHandoverService)
        {
            _cashHandoverService = cashHandoverService;
        }

        // =========================================================
        //  TRANG CHÍNH — Redirect về tab đúng
        // =========================================================

        public async Task<IActionResult> Index(int cashierId, string branchId = "CN001")
        {
            var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            // Bắt buộc dùng branchId của chính mình nếu không phải RManager
            if (!User.IsInRole("RManager"))
            {
                branchId = User.GetBranchId() ?? branchId;
            }

            // Nếu không phải quản lý, bắt buộc dùng cashierId của chính mình
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                cashierId = loggedInUserId;
            }

            // Nếu chưa truyền cashierId, lấy từ User đăng nhập hiện tại
            if (cashierId <= 0)
            {
                cashierId = loggedInUserId;
            }

            // Nếu vẫn không có cashierId (ví dụ Quản lý vào xem), chuyển sang trang chọn thu ngân
            if (cashierId <= 0)
            {
                return RedirectToAction(nameof(SelectCashier), new { branchId });
            }

            var activeModel = await _cashHandoverService.GetHandoverModelAsync(cashierId);
            if (activeModel != null)
                return RedirectToAction(nameof(Handover), new { cashierId });

            return RedirectToAction(nameof(OpenShift), new { cashierId });
        }

        // =========================================================
        //  CHỌN THU NGÂN — GET (khi chưa có cashierId)
        // =========================================================

        public async Task<IActionResult> SelectCashier(string branchId = "CN001")
        {
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                return Forbid();
            }

            if (!User.IsInRole("RManager"))
            {
                branchId = User.GetBranchId() ?? branchId;
            }

            var cashiers = await _cashHandoverService.GetCashiersAsync(branchId);
            ViewBag.BranchId = branchId;
            return View(cashiers);
        }

        // =========================================================
        //  MỞ CA (UC11) — GET
        // =========================================================

        public async Task<IActionResult> OpenShift(int cashierId)
        {
            var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                cashierId = loggedInUserId;
            }

            // Nếu không có cashierId hợp lệ → chuyển qua Index để tự tìm
            if (cashierId <= 0)
                return RedirectToAction(nameof(Index));

            try
            {
                var model = await _cashHandoverService.GetOpenShiftModelAsync(cashierId);
                if (model == null)
                {
                    // Ca đã được mở → chuyển sang form Giao ca
                    TempData["InfoMessage"] = "Ca làm việc hôm nay đã được mở. Vui lòng thực hiện Giao ca.";
                    return RedirectToAction(nameof(Handover), new { cashierId });
                }
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // =========================================================
        //  MỞ CA (UC11) — POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenShift(OpenShiftViewModel model)
        {
            var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                model.CashierId = loggedInUserId;
                model.BranchId = User.GetBranchId() ?? model.BranchId;
            }
            else if (!User.IsInRole("RManager"))
            {
                model.BranchId = User.GetBranchId() ?? model.BranchId;
            }

            if (!ModelState.IsValid)
            {
                var reloadModel = await _cashHandoverService.GetOpenShiftModelAsync(model.CashierId);
                if (reloadModel != null)
                {
                    model.CashierName = reloadModel.CashierName;
                    model.ShiftName = reloadModel.ShiftName;
                    model.ShiftTimeRange = reloadModel.ShiftTimeRange;
                    model.PreviousCashierName = reloadModel.PreviousCashierName;
                    model.PreviousShiftName = reloadModel.PreviousShiftName;
                    model.PreviousHandoverDate = reloadModel.PreviousHandoverDate;
                    model.PreviousInitialCash = reloadModel.PreviousInitialCash;
                    model.PreviousApproverName = reloadModel.PreviousApproverName;
                }
                return View(model);
            }

            var result = await _cashHandoverService.OpenShiftAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                var reloadModel = await _cashHandoverService.GetOpenShiftModelAsync(model.CashierId);
                if (reloadModel != null)
                {
                    model.CashierName = reloadModel.CashierName;
                    model.ShiftName = reloadModel.ShiftName;
                    model.ShiftTimeRange = reloadModel.ShiftTimeRange;
                    model.PreviousCashierName = reloadModel.PreviousCashierName;
                    model.PreviousShiftName = reloadModel.PreviousShiftName;
                    model.PreviousHandoverDate = reloadModel.PreviousHandoverDate;
                    model.PreviousInitialCash = reloadModel.PreviousInitialCash;
                    model.PreviousApproverName = reloadModel.PreviousApproverName;
                }
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Handover), new { cashierId = model.CashierId });
        }

        // =========================================================
        //  GIAO CA & ĐÓNG CA — GET
        // =========================================================

        public async Task<IActionResult> Handover(int cashierId)
        {
            var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                cashierId = loggedInUserId;
            }

            // Nếu không có cashierId hợp lệ → chuyển qua Index để tự tìm
            if (cashierId <= 0)
                return RedirectToAction(nameof(Index));

            var model = await _cashHandoverService.GetHandoverModelAsync(cashierId);
            if (model == null)
            {
                // Chưa mở ca → chuyển về Mở ca
                TempData["InfoMessage"] = "Chưa có ca làm việc đang mở. Vui lòng mở ca trước.";
                return RedirectToAction(nameof(OpenShift), new { cashierId });
            }
            return View(model);
        }

        // =========================================================
        //  GIAO CA & ĐÓNG CA — POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Handover(HandoverViewModel model)
        {
            var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                model.OutgoingCashierId = loggedInUserId;
                model.BranchId = User.GetBranchId() ?? model.BranchId;
            }
            else if (!User.IsInRole("RManager"))
            {
                model.BranchId = User.GetBranchId() ?? model.BranchId;
            }

            if (!ModelState.IsValid)
            {
                // Reload danh sách thu ngân khi model invalid
                var freshModel = await _cashHandoverService.GetHandoverModelAsync(model.OutgoingCashierId);
                if (freshModel != null)
                {
                    model.IncomingCashiers = freshModel.IncomingCashiers;
                    model.InitialCash = freshModel.InitialCash;
                    model.MachineCashRevenue = freshModel.MachineCashRevenue;
                    model.BankTransferRevenue = freshModel.BankTransferRevenue;
                }
                return View(model);
            }

            var result = await _cashHandoverService.CloseShiftAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                var freshModel = await _cashHandoverService.GetHandoverModelAsync(model.OutgoingCashierId);
                if (freshModel != null)
                {
                    model.IncomingCashiers = freshModel.IncomingCashiers;
                    model.InitialCash = freshModel.InitialCash;
                    model.MachineCashRevenue = freshModel.MachineCashRevenue;
                    model.BankTransferRevenue = freshModel.BankTransferRevenue;
                }
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(History), new { branchId = model.BranchId });
        }

        // =========================================================
        //  LỊCH SỬ GIAO CA — GET
        // =========================================================

        public async Task<IActionResult> History(string branchId, int page = 1)
        {
            var userBranchId = User.GetBranchId() ?? "";
            if (!User.IsInRole("RManager"))
            {
                branchId = userBranchId;
            }
            else if (string.IsNullOrEmpty(branchId))
            {
                branchId = userBranchId;
            }

            var model = await _cashHandoverService.GetHistoryAsync(branchId, page);
            return View(model);
        }
    }
}
