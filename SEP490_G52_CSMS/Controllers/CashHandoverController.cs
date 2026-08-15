using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Commons.Constants;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize]
    public class CashHandoverController : Controller
    {
        private readonly ICashHandoverService _cashHandoverService;
        private readonly CSMSAppDbContext _context;

        public CashHandoverController(ICashHandoverService cashHandoverService, CSMSAppDbContext context)
        {
            _cashHandoverService = cashHandoverService;
            _context = context;
        }

        private async Task<string> GetUserBranchIdAsync()
        {
            var branchIdClaim = User.GetBranchId();
            if (!string.IsNullOrWhiteSpace(branchIdClaim))
            {
                return branchIdClaim;
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == userId);
                if (employee != null && !string.IsNullOrWhiteSpace(employee.BranchId))
                {
                    return employee.BranchId;
                }
            }
            var firstBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Status == "Active");
            return firstBranch?.BranchId ?? "CB001";
        }

        // =========================================================
        //  TRANG CHÍNH — Tự động nhận diện và chuyển hướng đúng màn hình
        // =========================================================

        public async Task<IActionResult> Index(int cashierId, string? branchId = null)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int loggedInUserId = 0;
            int.TryParse(userIdStr, out loggedInUserId);

            var userBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("RManager") || string.IsNullOrWhiteSpace(branchId))
            {
                branchId = userBranchId;
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

        [HttpGet("/CashHandover/SubmitHandover")]
        public async Task<IActionResult> SubmitHandover(int cashierId, string? branchId = null)
        {
            return await Index(cashierId, branchId);
        }

        // =========================================================
        //  CHỌN THU NGÂN — GET
        // =========================================================

        public async Task<IActionResult> SelectCashier(string? branchId = null)
        {
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                return Forbid();
            }

            var userBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("RManager") || string.IsNullOrWhiteSpace(branchId))
            {
                branchId = userBranchId;
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

            var userBranchId = await GetUserBranchIdAsync();

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

            var userBranchId = await GetUserBranchIdAsync();

            if (cashierId <= 0)
                return RedirectToAction(nameof(History), new { branchId = userBranchId });

            var model = await _cashHandoverService.GetHandoverModelAsync(cashierId);
            if (model == null)
            {
                TempData["InfoMessage"] = "Chưa có ca làm việc đang mở. Vui lòng kiểm tra Lịch làm việc hoặc Mở ca.";
                return RedirectToAction(nameof(History), new { branchId = userBranchId });
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

            var phase = await _cashHandoverService.DetermineCurrentShiftPhaseAsync(cashierId, userBranchId);
            if (phase == CashHandoverConstants.HandoverTypeLastShift)
            {
                return RedirectToAction(nameof(CloseShift), new { cashierId });
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

            var userBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                model.OutgoingCashierId = loggedInUserId;
                model.BranchId = userBranchId;
            }
            else if (!User.IsInRole("RManager"))
            {
                model.BranchId = userBranchId;
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

            var userBranchId = await GetUserBranchIdAsync();

            if (cashierId <= 0)
                return RedirectToAction(nameof(History), new { branchId = userBranchId });

            var model = await _cashHandoverService.GetCloseShiftModelAsync(cashierId);
            if (model == null)
            {
                TempData["InfoMessage"] = "Không tìm thấy ca làm việc đang mở để đóng ca.";
                return RedirectToAction(nameof(History), new { branchId = userBranchId });
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

            var phase = await _cashHandoverService.DetermineCurrentShiftPhaseAsync(cashierId, userBranchId);
            if (phase != CashHandoverConstants.HandoverTypeLastShift)
            {
                return RedirectToAction(nameof(Handover), new { cashierId });
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

            var userBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                model.OutgoingCashierId = loggedInUserId;
                model.BranchId = userBranchId;
            }
            else if (!User.IsInRole("RManager"))
            {
                model.BranchId = userBranchId;
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

            var userBranchId = await GetUserBranchIdAsync();

            if (cashierId <= 0)
                return RedirectToAction(nameof(History), new { branchId = userBranchId });

            var model = await _cashHandoverService.GetEmergencyHandoverModelAsync(cashierId);
            if (model == null)
            {
                TempData["InfoMessage"] = "Không tìm thấy ca làm việc đang mở để bàn giao đột xuất.";
                return RedirectToAction(nameof(History), new { branchId = userBranchId });
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

            var userBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                model.OutgoingCashierId = loggedInUserId;
                model.BranchId = userBranchId;
            }
            else if (!User.IsInRole("RManager"))
            {
                model.BranchId = userBranchId;
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
            var userBranchId = await GetUserBranchIdAsync();
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
