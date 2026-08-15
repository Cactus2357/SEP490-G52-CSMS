using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Commons.Constants;
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
        //  TRANG CHÍNH — Tự động nhận diện và chuyển hướng đúng màn hình
        // =========================================================

        public async Task<IActionResult> Index(int cashierId, string branchId = "CN001")
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            if (!User.IsInRole("RManager"))
            {
                branchId = User.GetBranchId() ?? branchId;
            }

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                cashierId = loggedInUserId;
            }

            if (cashierId <= 0)
            {
                cashierId = loggedInUserId;
            }

            if (cashierId <= 0)
            {
                return RedirectToAction(nameof(SelectCashier), new { branchId });
            }

            // Nhận diện giai đoạn ca
            var phase = await _cashHandoverService.DetermineCurrentShiftPhaseAsync(cashierId, branchId);

            if (phase == CashHandoverConstants.HandoverTypeFirstShift)
            {
                var openModel = await _cashHandoverService.GetOpenShiftModelAsync(cashierId);
                if (openModel != null)
                {
                    return RedirectToAction(nameof(OpenShift), new { cashierId });
                }
                return RedirectToAction(nameof(Handover), new { cashierId });
            }
            else if (phase == CashHandoverConstants.HandoverTypeLastShift)
            {
                return RedirectToAction(nameof(CloseShift), new { cashierId });
            }
            else
            {
                return RedirectToAction(nameof(Handover), new { cashierId });
            }
        }

        // =========================================================
        //  CHỌN THU NGÂN — GET
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
        //  1. MỞ CA ĐẦU NGÀY (UC11) — GET & POST
        // =========================================================

        public async Task<IActionResult> OpenShift(int cashierId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                cashierId = loggedInUserId;
            }

            if (cashierId <= 0)
                return RedirectToAction("Index", "Home");

            var userBranchId = User.GetBranchId() ?? "CN001";

            try
            {
                var model = await _cashHandoverService.GetOpenShiftModelAsync(cashierId);
                if (model == null)
                {
                    var phase = await _cashHandoverService.DetermineCurrentShiftPhaseAsync(cashierId, userBranchId);
                    if (phase == CashHandoverConstants.HandoverTypeLastShift)
                    {
                        return RedirectToAction(nameof(CloseShift), new { cashierId });
                    }
                    return RedirectToAction(nameof(Handover), new { cashierId });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenShift(OpenShiftViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
            return RedirectToAction("CreateOrder", "SaleManagement");
        }

        // =========================================================
        //  2. BÀN GIAO CA GIỮA NGÀY (UC12) — GET & POST
        // =========================================================

        public async Task<IActionResult> Handover(int cashierId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                cashierId = loggedInUserId;
            }

            if (cashierId <= 0)
                return RedirectToAction(nameof(Index));

            var userBranchId = User.GetBranchId() ?? "CN001";
            var phase = await _cashHandoverService.DetermineCurrentShiftPhaseAsync(cashierId, userBranchId);
            if (phase == CashHandoverConstants.HandoverTypeLastShift)
            {
                TempData["InfoMessage"] = "Hiện đang là ca cuối cùng trong ngày (không có ca kế tiếp). Đã chuyển sang màn hình Đóng ca cuối ngày.";
                return RedirectToAction(nameof(CloseShift), new { cashierId });
            }

            var model = await _cashHandoverService.GetHandoverModelAsync(cashierId);
            if (model == null)
            {
                TempData["InfoMessage"] = "Chưa có ca làm việc đang mở hoặc ca hiện tại là ca cuối ngày. Vui lòng kiểm tra lại.";
                return RedirectToAction(nameof(Index), new { cashierId });
            }

            // Chỉ thu ngân đang trực mới được bàn giao ca
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                if (model.OutgoingCashierId != loggedInUserId)
                {
                    TempData["ErrorMessage"] = $"Bạn không phải là thu ngân đang phụ trách ca trực này ({model.OutgoingCashierName}). Chỉ thu ngân đang làm việc mới được bàn giao ca.";
                    return RedirectToAction(nameof(History), new { branchId = userBranchId });
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Handover(HandoverViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            decimal theoretical = model.InitialCash + model.MachineCashRevenue - model.CashRefundAmount;
            decimal discrepancy = model.ActualCash - theoretical;
            if (discrepancy != 0 && string.IsNullOrWhiteSpace(model.Notes))
            {
                ModelState.AddModelError("Notes", "Tiền két bị chênh lệch so với hệ thống. Vui lòng nhập lý do giải trình!");
            }

            if (!ModelState.IsValid)
            {
                var freshModel = await _cashHandoverService.GetHandoverModelAsync(model.OutgoingCashierId);
                if (freshModel != null)
                {
                    model.IncomingCashiers = freshModel.IncomingCashiers;
                    model.InitialCash = freshModel.InitialCash;
                    model.MachineCashRevenue = freshModel.MachineCashRevenue;
                    model.BankTransferRevenue = freshModel.BankTransferRevenue;
                    model.CashRefundAmount = freshModel.CashRefundAmount;
                    model.OutgoingCashierName = freshModel.OutgoingCashierName;
                    model.ShiftName = freshModel.ShiftName;
                    model.TargetShiftName = freshModel.TargetShiftName;
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
                    model.CashRefundAmount = freshModel.CashRefundAmount;
                    model.OutgoingCashierName = freshModel.OutgoingCashierName;
                    model.ShiftName = freshModel.ShiftName;
                    model.TargetShiftName = freshModel.TargetShiftName;
                }
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(History), new { branchId = model.BranchId });
        }

        // =========================================================
        //  3. ĐÓNG CA CUỐI NGÀY — GET & POST
        // =========================================================

        public async Task<IActionResult> CloseShift(int cashierId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                cashierId = loggedInUserId;
            }

            if (cashierId <= 0)
                return RedirectToAction(nameof(Index));

            var userBranchId = User.GetBranchId() ?? "CN001";
            var phase = await _cashHandoverService.DetermineCurrentShiftPhaseAsync(cashierId, userBranchId);
            if (phase != CashHandoverConstants.HandoverTypeLastShift)
            {
                TempData["InfoMessage"] = "Màn hình Đóng ca cuối ngày chỉ áp dụng cho ca cuối cùng trong ngày.";
                return RedirectToAction(nameof(Handover), new { cashierId });
            }

            var model = await _cashHandoverService.GetCloseShiftModelAsync(cashierId);
            if (model == null)
            {
                TempData["InfoMessage"] = "Không tìm thấy ca làm việc đang mở để đóng cuối ngày.";
                return RedirectToAction(nameof(Index), new { cashierId });
            }

            // Chỉ thu ngân đang trực mới được đóng ca
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                if (model.OutgoingCashierId != loggedInUserId)
                {
                    TempData["ErrorMessage"] = $"Bạn không phải là thu ngân đang phụ trách ca trực này ({model.CashierName}). Chỉ thu ngân đang làm việc mới được đóng ca.";
                    return RedirectToAction(nameof(History), new { branchId = userBranchId });
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseShift(CloseShiftViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            decimal theoretical = model.InitialCash + model.MachineCashRevenue - model.CashRefundAmount;
            decimal discrepancy = model.ActualCash - theoretical;
            if (discrepancy != 0 && string.IsNullOrWhiteSpace(model.Notes))
            {
                ModelState.AddModelError("Notes", "Tiền két cuối ngày bị chênh lệch. Vui lòng nhập lý do giải trình!");
            }

            if (!ModelState.IsValid)
            {
                var freshModel = await _cashHandoverService.GetCloseShiftModelAsync(model.OutgoingCashierId);
                if (freshModel != null)
                {
                    model.InitialCash = freshModel.InitialCash;
                    model.MachineCashRevenue = freshModel.MachineCashRevenue;
                    model.BankTransferRevenue = freshModel.BankTransferRevenue;
                    model.CashRefundAmount = freshModel.CashRefundAmount;
                    model.CashierName = freshModel.CashierName;
                    model.ShiftName = freshModel.ShiftName;
                    model.ShiftTimeRange = freshModel.ShiftTimeRange;
                }
                return View(model);
            }

            var result = await _cashHandoverService.CloseShiftOfDayAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                var freshModel = await _cashHandoverService.GetCloseShiftModelAsync(model.OutgoingCashierId);
                if (freshModel != null)
                {
                    model.InitialCash = freshModel.InitialCash;
                    model.MachineCashRevenue = freshModel.MachineCashRevenue;
                    model.BankTransferRevenue = freshModel.BankTransferRevenue;
                    model.CashRefundAmount = freshModel.CashRefundAmount;
                    model.CashierName = freshModel.CashierName;
                    model.ShiftName = freshModel.ShiftName;
                    model.ShiftTimeRange = freshModel.ShiftTimeRange;
                }
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(History), new { branchId = model.BranchId });
        }

        // =========================================================
        //  4. BÀN GIAO ĐỘT XUẤT GIỮA CA (Ốm / Khẩn cấp) — GET & POST
        // =========================================================

        public async Task<IActionResult> EmergencyHandover(int cashierId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                cashierId = loggedInUserId;
            }

            if (cashierId <= 0)
                return RedirectToAction(nameof(Index));

            var userBranchId = User.GetBranchId() ?? "CN001";
            var model = await _cashHandoverService.GetEmergencyHandoverModelAsync(cashierId);
            if (model == null)
            {
                TempData["InfoMessage"] = "Không tìm thấy ca làm việc đang mở để bàn giao đột xuất.";
                return RedirectToAction(nameof(Index), new { cashierId });
            }

            // Chỉ thu ngân đang trực mới được bàn giao đột xuất
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                if (model.OutgoingCashierId != loggedInUserId)
                {
                    TempData["ErrorMessage"] = $"Bạn không phải là thu ngân đang phụ trách ca trực này ({model.OutgoingCashierName}). Chỉ thu ngân đang làm việc mới được bàn giao đột xuất.";
                    return RedirectToAction(nameof(History), new { branchId = userBranchId });
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmergencyHandover(HandoverViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            if (string.IsNullOrWhiteSpace(model.EmergencyReason))
            {
                ModelState.AddModelError("EmergencyReason", "Vui lòng nhập lý do bàn giao đột xuất (ốm/việc gấp/khác).");
            }

            if (!ModelState.IsValid)
            {
                var freshModel = await _cashHandoverService.GetEmergencyHandoverModelAsync(model.OutgoingCashierId);
                if (freshModel != null)
                {
                    model.IncomingCashiers = freshModel.IncomingCashiers;
                    model.InitialCash = freshModel.InitialCash;
                    model.MachineCashRevenue = freshModel.MachineCashRevenue;
                    model.BankTransferRevenue = freshModel.BankTransferRevenue;
                    model.CashRefundAmount = freshModel.CashRefundAmount;
                    model.OutgoingCashierName = freshModel.OutgoingCashierName;
                    model.ShiftName = freshModel.ShiftName;
                }
                return View(model);
            }

            var result = await _cashHandoverService.EmergencyHandoverAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                var freshModel = await _cashHandoverService.GetEmergencyHandoverModelAsync(model.OutgoingCashierId);
                if (freshModel != null)
                {
                    model.IncomingCashiers = freshModel.IncomingCashiers;
                    model.InitialCash = freshModel.InitialCash;
                    model.MachineCashRevenue = freshModel.MachineCashRevenue;
                    model.BankTransferRevenue = freshModel.BankTransferRevenue;
                    model.CashRefundAmount = freshModel.CashRefundAmount;
                    model.OutgoingCashierName = freshModel.OutgoingCashierName;
                    model.ShiftName = freshModel.ShiftName;
                }
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(History), new { branchId = model.BranchId });
        }

        // =========================================================
        //  5. LỊCH SỬ GIAO CA — GET
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
