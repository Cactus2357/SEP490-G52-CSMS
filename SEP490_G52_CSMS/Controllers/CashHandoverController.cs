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
        private readonly ICashierWorkEligibilityService? _eligibilityService;

        public CashHandoverController(
            ICashHandoverService cashHandoverService, 
            CSMSAppDbContext context,
            ICashierWorkEligibilityService? eligibilityService = null)
        {
            _cashHandoverService = cashHandoverService;
            _context = context;
            _eligibilityService = eligibilityService;
        }

        private async Task<(bool isEligible, string reasonCode, string message)> CheckCashierEligibilityAsync(int employeeId, string branchId)
        {
            if (_eligibilityService == null || User.IsInRole("RManager") || User.IsInRole("BranchManager"))
            {
                return (true, "Eligible", "Hợp lệ");
            }

            var result = await _eligibilityService.CheckEligibilityAsync(employeeId, branchId);
            return (result.IsEligible, result.ReasonCode, result.Message);
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

        public async Task<IActionResult> Index(int? cashierId = null, string? branchId = null)
        {
            int loggedInUserId = User.GetEmployeeId() ?? 0;
            var userBranchId = await GetUserBranchIdAsync();

            if (!User.IsInRole("RManager") || string.IsNullOrWhiteSpace(branchId))
            {
                branchId = userBranchId;
            }

            int effectiveCashierId = (cashierId.HasValue && cashierId.Value > 0 && (User.IsInRole("BranchManager") || User.IsInRole("RManager")))
                ? cashierId.Value
                : loggedInUserId;

            if (effectiveCashierId <= 0)
            {
                return RedirectToAction(nameof(SelectCashier));
            }

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == loggedInUserId);
                if (employee == null || (employee.Role != CashHandoverConstants.CashierRole && employee.Role != "Cashier"))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập quản lý ca thu ngân (Chỉ dành cho nhân viên Thu ngân).";
                    return RedirectToAction("Index", "Home");
                }

                var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(effectiveCashierId, branchId);
                if (!isEligible && _eligibilityService != null)
                {
                    if (reasonCode == "FirstShiftNotOpened")
                    {
                        return RedirectToAction(nameof(OpenShift));
                    }
                    if (reasonCode == "MidShiftNotHandedOver")
                    {
                        var openModel = await _cashHandoverService.GetOpenShiftModelAsync(effectiveCashierId);
                        if (openModel != null)
                        {
                            return RedirectToAction(nameof(OpenShift));
                        }
                        TempData["ErrorMessage"] = message;
                        return RedirectToAction(nameof(History));
                    }

                    TempData["ErrorMessage"] = message;
                    return RedirectToAction(nameof(History));
                }
            }

            // Nhận diện giai đoạn ca
            var phase = await _cashHandoverService.DetermineCurrentShiftPhaseAsync(effectiveCashierId, branchId);

            if (phase == CashHandoverConstants.HandoverTypeFirstShift)
            {
                var openModel = await _cashHandoverService.GetOpenShiftModelAsync(effectiveCashierId);
                if (openModel != null)
                {
                    return RedirectToAction(nameof(OpenShift));
                }
                return RedirectToAction(nameof(Handover));
            }
            else if (phase == CashHandoverConstants.HandoverTypeLastShift)
            {
                return RedirectToAction(nameof(CloseShift));
            }
            else
            {
                return RedirectToAction(nameof(Handover));
            }
        }

        [HttpGet("/CashHandover/SubmitHandover")]
        public async Task<IActionResult> SubmitHandover(int? cashierId = null, string? branchId = null)
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

        public async Task<IActionResult> OpenShift(int? cashierId = null)
        {
            int loggedInUserId = User.GetEmployeeId() ?? 0;

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == loggedInUserId);
                if (employee == null || (employee.Role != CashHandoverConstants.CashierRole && employee.Role != "Cashier"))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền thực hiện mở ca thu ngân (Chỉ dành cho nhân viên Thu ngân).";
                    return RedirectToAction("Index", "Home");
                }
                cashierId = loggedInUserId;
            }

            int targetCashierId = (cashierId.HasValue && cashierId.Value > 0) ? cashierId.Value : loggedInUserId;
            if (targetCashierId <= 0)
                return RedirectToAction("Index", "Home");

            var userBranchId = await GetUserBranchIdAsync();

            if (_eligibilityService != null && !User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(targetCashierId, userBranchId);
                if (isEligible)
                {
                    TempData["InfoMessage"] = "Ca làm việc của bạn hiện đang mở và đang hoạt động.";
                    return RedirectToAction("CreateOrder", "SaleManagement");
                }

                if (reasonCode != "FirstShiftNotOpened" && reasonCode != "MidShiftNotHandedOver")
                {
                    TempData["ErrorMessage"] = message;
                    return RedirectToAction(nameof(History));
                }
            }

            try
            {
                var model = await _cashHandoverService.GetOpenShiftModelAsync(targetCashierId);
                if (model == null)
                {
                    var isDayClosed = await _cashHandoverService.IsDayClosedAsync(userBranchId, DateTime.Today);
                    if (isDayClosed)
                    {
                        TempData["InfoMessage"] = "Ca làm việc cuối ngày hôm nay tại chi nhánh đã được Đóng ca (chốt sổ ngày). Quầy thu ngân đã đóng cửa, không thể thao tác thêm ca.";
                        return RedirectToAction(nameof(History));
                    }

                    TempData["ErrorMessage"] = "Không thể mở ca làm việc. Vui lòng kiểm tra lại Lịch làm việc và Chấm công.";
                    return RedirectToAction(nameof(History));
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

            if (_eligibilityService != null && !User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(model.CashierId, model.BranchId);
                if (reasonCode != "FirstShiftNotOpened" && reasonCode != "MidShiftNotHandedOver")
                {
                    ModelState.AddModelError(string.Empty, message);
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

        public async Task<IActionResult> Handover(int? cashierId = null)
        {
            int loggedInUserId = User.GetEmployeeId() ?? 0;

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == loggedInUserId);
                if (employee == null || (employee.Role != CashHandoverConstants.CashierRole && employee.Role != "Cashier"))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền thực hiện bàn giao ca thu ngân (Chỉ dành cho nhân viên Thu ngân).";
                    return RedirectToAction("Index", "Home");
                }
                cashierId = loggedInUserId;
            }

            int targetCashierId = (cashierId.HasValue && cashierId.Value > 0) ? cashierId.Value : loggedInUserId;
            var userBranchId = await GetUserBranchIdAsync();

            if (targetCashierId <= 0)
                return RedirectToAction(nameof(History));

            if (_eligibilityService != null && !User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(targetCashierId, userBranchId);
                if (!isEligible)
                {
                    if (reasonCode == "FirstShiftNotOpened")
                    {
                        TempData["InfoMessage"] = "Chưa có ca làm việc nào được mở hôm nay. Vui lòng mở ca trước.";
                        return RedirectToAction(nameof(OpenShift));
                    }

                    TempData["ErrorMessage"] = message;
                    return RedirectToAction(nameof(History));
                }
            }

            var model = await _cashHandoverService.GetHandoverModelAsync(targetCashierId);
            if (model == null)
            {
                TempData["InfoMessage"] = "Chưa có ca làm việc đang mở. Vui lòng kiểm tra Lịch làm việc hoặc Mở ca.";
                return RedirectToAction(nameof(History));
            }

            // Chỉ thu ngân đang trực mới được bàn giao ca
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                if (model.OutgoingCashierId != loggedInUserId)
                {
                    TempData["ErrorMessage"] = $"Bạn không phải là thu ngân đang phụ trách ca trực này ({model.OutgoingCashierName}). Chỉ thu ngân đang làm việc mới được bàn giao ca.";
                    return RedirectToAction(nameof(History));
                }
            }

            var phase = await _cashHandoverService.DetermineCurrentShiftPhaseAsync(targetCashierId, userBranchId);
            if (phase == CashHandoverConstants.HandoverTypeLastShift)
            {
                return RedirectToAction(nameof(CloseShift));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Handover(HandoverViewModel model)
        {
            int loggedInUserId = User.GetEmployeeId() ?? 0;
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

            if (_eligibilityService != null && !User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(model.OutgoingCashierId, model.BranchId);
                if (!isEligible)
                {
                    ModelState.AddModelError(string.Empty, message);
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
                        model.TargetShiftTimeRange = freshModel.TargetShiftTimeRange;
                        model.DelivererName = freshModel.DelivererName;
                    }
                    return View(model);
                }
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
                    model.TargetShiftTimeRange = freshModel.TargetShiftTimeRange;
                    model.DelivererName = freshModel.DelivererName;
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
                    model.TargetShiftTimeRange = freshModel.TargetShiftTimeRange;
                    model.DelivererName = freshModel.DelivererName;
                }
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(History));
        }

        // =========================================================
        //  3. ĐÓNG CA CUỐI NGÀY — GET & POST
        // =========================================================

        private async Task<List<string>> GetReceiverSuggestionsAsync(string branchId)
        {
            var suggestions = new List<string>
            {
                "Két tổng chi nhánh",
                "Chủ quán",
                "Quản lý chi nhánh"
            };

            var branchStaff = await _context.Employees
                .Where(e => (e.BranchId == branchId || e.Role == "BranchManager" || e.Role == "RManager") && e.Status == "Active")
                .OrderBy(e => e.FullName)
                .Select(e => e.FullName)
                .Distinct()
                .ToListAsync();

            foreach (var staff in branchStaff)
            {
                if (!string.IsNullOrWhiteSpace(staff) && !suggestions.Contains(staff))
                {
                    suggestions.Add(staff);
                }
            }

            return suggestions;
        }

        public async Task<IActionResult> CloseShift(int? cashierId = null)
        {
            int loggedInUserId = User.GetEmployeeId() ?? 0;

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == loggedInUserId);
                if (employee == null || (employee.Role != CashHandoverConstants.CashierRole && employee.Role != "Cashier"))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền thực hiện đóng ca thu ngân (Chỉ dành cho nhân viên Thu ngân).";
                    return RedirectToAction("Index", "Home");
                }
                cashierId = loggedInUserId;
            }

            int targetCashierId = (cashierId.HasValue && cashierId.Value > 0) ? cashierId.Value : loggedInUserId;
            var userBranchId = await GetUserBranchIdAsync();

            if (targetCashierId <= 0)
                return RedirectToAction(nameof(History));

            if (_eligibilityService != null && !User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(targetCashierId, userBranchId);
                if (!isEligible)
                {
                    if (reasonCode == "FirstShiftNotOpened")
                    {
                        TempData["InfoMessage"] = "Chưa có ca làm việc nào được mở hôm nay. Vui lòng mở ca trước.";
                        return RedirectToAction(nameof(OpenShift));
                    }

                    TempData["ErrorMessage"] = message;
                    return RedirectToAction(nameof(History));
                }
            }

            var model = await _cashHandoverService.GetCloseShiftModelAsync(targetCashierId);
            if (model == null)
            {
                TempData["InfoMessage"] = "Không tìm thấy ca làm việc đang mở để đóng ca.";
                return RedirectToAction(nameof(History));
            }

            // Chỉ thu ngân đang trực mới được đóng ca
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                if (model.OutgoingCashierId != loggedInUserId)
                {
                    TempData["ErrorMessage"] = $"Bạn không phải là thu ngân đang phụ trách ca trực này ({model.CashierName}). Chỉ thu ngân đang làm việc mới được đóng ca.";
                    return RedirectToAction(nameof(History));
                }
            }

            var phase = await _cashHandoverService.DetermineCurrentShiftPhaseAsync(targetCashierId, userBranchId);
            if (phase != CashHandoverConstants.HandoverTypeLastShift)
            {
                return RedirectToAction(nameof(Handover));
            }

            ViewBag.ReceiverSuggestions = await GetReceiverSuggestionsAsync(userBranchId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseShift(CloseShiftViewModel model)
        {
            int loggedInUserId = User.GetEmployeeId() ?? 0;
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

            if (_eligibilityService != null && !User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(model.OutgoingCashierId, model.BranchId);
                if (!isEligible)
                {
                    ModelState.AddModelError(string.Empty, message);
                    var freshModel = await _cashHandoverService.GetCloseShiftModelAsync(model.OutgoingCashierId);
                    if (freshModel != null)
                    {
                        model.InitialCash = freshModel.InitialCash;
                        model.MachineCashRevenue = freshModel.MachineCashRevenue;
                        model.BankTransferRevenue = freshModel.BankTransferRevenue;
                        model.CashRefundAmount = 0;
                        model.CashierName = freshModel.CashierName;
                        model.ShiftName = freshModel.ShiftName;
                        model.ShiftTimeRange = freshModel.ShiftTimeRange;
                    }
                    ViewBag.ReceiverSuggestions = await GetReceiverSuggestionsAsync(userBranchId);
                    return View(model);
                }
            }

            if (string.IsNullOrWhiteSpace(model.Notes))
            {
                ModelState.AddModelError("Notes", "Vui lòng nhập lý do / ghi chú chênh lệch.");
            }

            if (!ModelState.IsValid)
            {
                var freshModel = await _cashHandoverService.GetCloseShiftModelAsync(model.OutgoingCashierId);
                if (freshModel != null)
                {
                    model.InitialCash = freshModel.InitialCash;
                    model.MachineCashRevenue = freshModel.MachineCashRevenue;
                    model.BankTransferRevenue = freshModel.BankTransferRevenue;
                    model.CashRefundAmount = 0;
                    model.CashierName = freshModel.CashierName;
                    model.ShiftName = freshModel.ShiftName;
                    model.ShiftTimeRange = freshModel.ShiftTimeRange;
                }
                ViewBag.ReceiverSuggestions = await GetReceiverSuggestionsAsync(userBranchId);
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
                    model.CashRefundAmount = 0;
                    model.CashierName = freshModel.CashierName;
                    model.ShiftName = freshModel.ShiftName;
                    model.ShiftTimeRange = freshModel.ShiftTimeRange;
                }
                ViewBag.ReceiverSuggestions = await GetReceiverSuggestionsAsync(userBranchId);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(History));
        }

        // =========================================================
        //  4. BÀN GIAO ĐỘT XUẤT GIỮA CA (Ốm / Khẩn cấp) — GET & POST
        // =========================================================

        public async Task<IActionResult> EmergencyHandover(int? cashierId = null)
        {
            int loggedInUserId = User.GetEmployeeId() ?? 0;

            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == loggedInUserId);
                if (employee == null || (employee.Role != CashHandoverConstants.CashierRole && employee.Role != "Cashier"))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền thực hiện bàn giao đột xuất ca thu ngân (Chỉ dành cho nhân viên Thu ngân).";
                    return RedirectToAction("Index", "Home");
                }
                cashierId = loggedInUserId;
            }

            int targetCashierId = (cashierId.HasValue && cashierId.Value > 0) ? cashierId.Value : loggedInUserId;
            var userBranchId = await GetUserBranchIdAsync();

            if (targetCashierId <= 0)
                return RedirectToAction(nameof(History));

            if (_eligibilityService != null && !User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(targetCashierId, userBranchId);
                if (!isEligible)
                {
                    TempData["ErrorMessage"] = message;
                    return RedirectToAction(nameof(History));
                }
            }

            var model = await _cashHandoverService.GetEmergencyHandoverModelAsync(targetCashierId);
            if (model == null && !string.IsNullOrEmpty(userBranchId))
            {
                var today = DateTime.UtcNow.Date;
                var branchActiveHandover = await _context.CashHandovers
                    .AsNoTracking()
                    .Where(h => h.BranchId == userBranchId
                             && h.HandoverDate.Date == today
                             && h.Status == CashHandoverConstants.ActiveStatus)
                    .OrderByDescending(h => h.OpenedAt)
                    .FirstOrDefaultAsync();

                if (branchActiveHandover != null)
                {
                    model = await _cashHandoverService.GetEmergencyHandoverModelAsync(branchActiveHandover.OutgoingCashierId);
                }
            }

            if (model == null)
            {
                TempData["InfoMessage"] = "Không tìm thấy ca làm việc đang mở để bàn giao đột xuất.";
                return RedirectToAction(nameof(History));
            }

            // Chỉ thu ngân đang trực mới được bàn giao đột xuất
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                if (model.OutgoingCashierId != loggedInUserId)
                {
                    TempData["ErrorMessage"] = $"Bạn không phải là thu ngân đang phụ trách ca trực này ({model.OutgoingCashierName}). Chỉ thu ngân đang làm việc mới được bàn giao đột xuất.";
                    return RedirectToAction(nameof(History));
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmergencyHandover(HandoverViewModel model)
        {
            int loggedInUserId = User.GetEmployeeId() ?? 0;
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

            if (_eligibilityService != null && !User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(model.OutgoingCashierId, model.BranchId);
                if (!isEligible)
                {
                    ModelState.AddModelError(string.Empty, message);
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
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                return RedirectToAction("CreateOrder", "SaleManagement");
            }
            return RedirectToAction(nameof(History));
        }

        // =========================================================
        //  5. LỊCH SỬ GIAO CA — GET
        // =========================================================

        public async Task<IActionResult> History(string? branchId = null, int page = 1)
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
