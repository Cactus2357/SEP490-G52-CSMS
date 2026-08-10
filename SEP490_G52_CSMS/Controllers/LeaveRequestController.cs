using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services;
using System.Security.Claims;
using SEP490_G52_CSMS.Commons;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly CSMSAppDbContext _context;

        public LeaveRequestController(ILeaveRequestService leaveRequestService, CSMSAppDbContext context)
        {
            _leaveRequestService = leaveRequestService;
            _context = context;
        }

        private async Task<(int Id, string Name, string Role, string BranchId, string BranchName)> GetCurrentUserAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employee = await _context.Employees.Include(e => e.Branch).FirstOrDefaultAsync(e => e.EmployeeId == userId);
                if (employee != null)
                {
                    return (employee.EmployeeId, employee.FullName ?? employee.Username ?? "", employee.Role ?? "Thu ngân", employee.BranchId ?? "", employee.Branch?.BranchName ?? "Chi nhánh");
                }
            }
            return (4, "Nguyễn Văn A", "Thu ngân", "", "Chi nhánh 1");
        }

        // --- STAFF LEAVE APPLICATION VIEWS ---

        [Authorize(Roles = "Cashier,Bartender,Busser,Barista,Staff,Employee")]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string status)
        {
            var currentUser = await GetCurrentUserAsync();
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                var today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                fromDate = today.AddDays(-1 * diff).Date;
                toDate = fromDate.Value.AddDays(6).Date;
            }

            var model = await _leaveRequestService.GetLeaveRequestListAsync(currentUser.Id, currentUser.Name, fromDate, toDate, status);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Cashier,Bartender,Busser,Barista,Staff,Employee")]
        public async Task<IActionResult> Create(LeaveRequestCreateViewModel model)
        {
            var currentUser = await GetCurrentUserAsync();
            model.EmployeeId = currentUser.Id;
            model.EmployeeName = currentUser.Name;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _leaveRequestService.CreateLeaveRequestAsync(model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Cashier,Bartender,Busser,Barista,Staff,Employee")]
        public async Task<IActionResult> Cancel(int applicationId)
        {
            var currentUser = await GetCurrentUserAsync();
            var result = await _leaveRequestService.CancelLeaveRequestAsync(applicationId, currentUser.Id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // --- BRANCH MANAGER LEAVE MANAGEMENT (UC47 & UC48) ---

        [HttpGet]
        [Authorize(Roles = "BranchManager,RManager")]
        public async Task<IActionResult> ManagerIndex(string searchName, DateTime? fromDate, DateTime? toDate, string status = "Tất cả")
        {
            var (userId, userName, role, branchId, branchName) = await GetCurrentUserAsync();
            if (string.IsNullOrEmpty(branchId))
            {
                branchId = User.GetBranchId() ?? "";
            }

            var model = await _leaveRequestService.GetManagerLeaveRequestsAsync(branchId, branchName, searchName, fromDate, toDate, status);
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "BranchManager,RManager")]
        public async Task<IActionResult> GetDetailModal(int id)
        {
            var (userId, userName, role, branchId, branchName) = await GetCurrentUserAsync();
            if (string.IsNullOrEmpty(branchId))
            {
                branchId = User.GetBranchId() ?? "";
            }

            var detail = await _leaveRequestService.GetLeaveRequestDetailAsync(id, branchId);
            if (detail == null)
            {
                return NotFound("Không tìm thấy thông tin đơn xin nghỉ phép hoặc không thuộc chi nhánh của bạn.");
            }

            return PartialView("_ManagerDetailModal", detail);
        }

        [HttpPost]
        [Authorize(Roles = "BranchManager,RManager")]
        public async Task<IActionResult> Approve(int id)
        {
            var (userId, userName, role, branchId, branchName) = await GetCurrentUserAsync();
            if (string.IsNullOrEmpty(branchId))
            {
                branchId = User.GetBranchId() ?? "";
            }

            var result = await _leaveRequestService.ApproveLeaveRequestAsync(id, branchId, userId, role);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [Authorize(Roles = "BranchManager,RManager")]
        public async Task<IActionResult> Reject(int id, string? reason)
        {
            var (userId, userName, role, branchId, branchName) = await GetCurrentUserAsync();
            if (string.IsNullOrEmpty(branchId))
            {
                branchId = User.GetBranchId() ?? "";
            }

            var result = await _leaveRequestService.RejectLeaveRequestAsync(id, branchId, userId, role, reason);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
