using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Commons.Constants;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services;

namespace SEP490_G52_CSMS.Controllers
{
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
            // Nếu chưa có cashierId, tìm người đang trực hoặc sắp trực
            if (cashierId <= 0)
            {
                var autoCashierId = await _cashHandoverService.GetCurrentCashierIdAsync(branchId);
                if (autoCashierId.HasValue)
                {
                    cashierId = autoCashierId.Value;
                }
                else
                {
                    // Vẫn không tìm được ai thì chuyển sang trang chọn thu ngân
                    return RedirectToAction(nameof(SelectCashier), new { branchId });
                }
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
            var cashiers = await _cashHandoverService.GetCashiersAsync(branchId);
            ViewBag.BranchId = branchId;
            return View(cashiers);
        }

        // =========================================================
        //  MỞ CA (UC11) — GET
        // =========================================================

        public async Task<IActionResult> OpenShift(int cashierId)
        {
            // Nếu không có cashierId hợp lệ → chuyển qua Index để tự tìm
            if (cashierId <= 0)
                return RedirectToAction(nameof(Index));

            var model = await _cashHandoverService.GetOpenShiftModelAsync(cashierId);
            if (model == null)
            {
                // Ca đã được mở → chuyển sang form Giao ca
                TempData["InfoMessage"] = "Ca làm việc hôm nay đã được mở. Vui lòng thực hiện Giao ca.";
                return RedirectToAction(nameof(Handover), new { cashierId });
            }
            return View(model);
        }

        // =========================================================
        //  MỞ CA (UC11) — POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenShift(OpenShiftViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _cashHandoverService.OpenShiftAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
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
            var model = await _cashHandoverService.GetHistoryAsync(branchId, page);
            return View(model);
        }
    }
}
