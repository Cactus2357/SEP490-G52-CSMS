using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ShiftChangeController : Controller
    {
        private const int PageSize = 8;
        private readonly CSMSAppDbContext _context;
        private readonly IShiftChangeService _shiftChangeService;

        public ShiftChangeController(CSMSAppDbContext context, IShiftChangeService shiftChangeService)
        {
            _context = context;
            _shiftChangeService = shiftChangeService;
        }

        // ── Helpers lấy thông tin người đang đăng nhập ──────────────────────────

        private int GetCurrentEmployeeId()
        {
            var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(val, out var id) ? id : 0;
        }

        private string GetCurrentRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        private string GetCurrentFullName() =>
            User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        /// <summary>Lấy BranchId của nhân viên đang đăng nhập từ DB.</summary>
        private async Task<string> GetCurrentBranchIdAsync()
        {
            var empId = GetCurrentEmployeeId();
            if (empId == 0) return string.Empty;
            var emp = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == empId);
            return emp?.BranchId ?? string.Empty;
        }

        // ============================================================
        // PHẦN QUẢN LÝ (Branch Manager) — xem & duyệt đơn của nhân viên
        // ============================================================

        [Authorize(Roles = "BranchManager")]
        [HttpGet]
        public async Task<IActionResult> Index(int? employeeId = null, string? status = null, string? keyword = null, int page = 1)
        {
            var branchId = User.GetBranchId() ?? "";

            IQueryable<ShiftChangeRequest> query = _context.ShiftChangeRequests
                .Include(r => r.RequestingEmployee)
                .Where(r => r.RequestingEmployee != null && r.RequestingEmployee.BranchId == branchId);

            if (employeeId.HasValue)
                query = query.Where(r => r.RequestingEmployeeId == employeeId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(r =>
                    (r.RequestingEmployee != null && (
                        (r.RequestingEmployee.FullName != null && r.RequestingEmployee.FullName.ToLower().Contains(kw)) ||
                        (r.RequestingEmployee.Username != null && r.RequestingEmployee.Username.ToLower().Contains(kw)) ||
                        (r.RequestingEmployee.PhoneNumber != null && r.RequestingEmployee.PhoneNumber.Contains(kw))
                    )) ||
                    (r.Reason != null && r.Reason.ToLower().Contains(kw)) ||
                    (r.Aspiration != null && r.Aspiration.ToLower().Contains(kw))
                );
            }

            query = query.OrderByDescending(r => r.SubmittedAt);

            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            var requests = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var employeeOptions = await _context.Employees
                .Where(e => e.BranchId == branchId && e.Role != "BranchManager")
                .OrderBy(e => e.FullName)
                .ToListAsync();

            var vm = new ManagerShiftChangeListViewModel
            {
                Requests = requests,
                EmployeeOptions = employeeOptions,
                SelectedEmployeeId = employeeId,
                SelectedStatus = status,
                Keyword = keyword?.Trim(),
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(vm);
        }

        [Authorize(Roles = "BranchManager")]
        [HttpGet]
        public async Task<IActionResult> DetailPartial(int id)
        {
            var branchId = User.GetBranchId() ?? "";

            var request = await _context.ShiftChangeRequests
                .Include(r => r.RequestingEmployee)
                .FirstOrDefaultAsync(r => r.RequestId == id
                    && r.RequestingEmployee != null
                    && r.RequestingEmployee.BranchId == branchId);

            if (request == null)
                return NotFound();

            if (request.RequestingEmployee == null || request.RequestingEmployee.BranchId != branchId)
            {
                return Forbid();
            }

            return PartialView("_ShiftChangeDetail", request);
        }

        [Authorize(Roles = "BranchManager")]
        [HttpPost]
        public async Task<IActionResult> Approve([FromBody] ShiftDecisionRequest req)
        {
            if (req == null)
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });

            var request = await _context.ShiftChangeRequests
                .Include(r => r.RequestingEmployee)
                .FirstOrDefaultAsync(r => r.RequestId == req.RequestId);

            if (request == null)
                return Json(new { success = false, errorMessage = "Không tìm thấy đơn yêu cầu." });

            var branchId = User.GetBranchId() ?? "";
            if (request.RequestingEmployee == null || request.RequestingEmployee.BranchId != branchId)
            {
                return Forbid();
            }

            if (request.Status != "Submitted")
                return Json(new { success = false, errorMessage = "Đơn này đã được xử lý trước đó." });

            request.Status = "Approved";
            request.ApprovedBranchId = branchId;

            var managerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(managerIdClaim, out int managerId))
            {
                request.ApprovedManagerId = managerId;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Phê duyệt đơn đổi ca thành công!" });
        }

        [Authorize(Roles = "BranchManager")]
        [HttpPost]
        public async Task<IActionResult> Reject([FromBody] ShiftDecisionRequest req)
        {
            if (req == null)
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });

            var request = await _context.ShiftChangeRequests
                .Include(r => r.RequestingEmployee)
                .FirstOrDefaultAsync(r => r.RequestId == req.RequestId);

            if (request == null)
                return Json(new { success = false, errorMessage = "Không tìm thấy đơn yêu cầu." });

            var branchId = User.GetBranchId() ?? "";
            if (request.RequestingEmployee == null || request.RequestingEmployee.BranchId != branchId)
            {
                return Forbid();
            }

            if (request.Status != "Submitted")
                return Json(new { success = false, errorMessage = "Đơn này đã được xử lý trước đó." });

            request.Status = "Rejected";
            request.ApprovedBranchId = branchId;

            var managerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(managerIdClaim, out int managerId))
            {
                request.ApprovedManagerId = managerId;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã từ chối đơn đổi ca." });
        }

        // ============================================================
        // PHẦN NHÂN VIÊN — xem & tạo đơn đổi ca của chính mình
        // ============================================================

        [Authorize(Roles = "Cashier,Bartender,Busser,Barista,Staff,Employee")]
        [HttpGet]
        public async Task<IActionResult> EmployeeIndex(DateTime? fromDate, DateTime? toDate, string? status)
        {
            var empId = GetCurrentEmployeeId();
            var empName = GetCurrentFullName();
            var empRole = GetCurrentRole();

            // Mặc định load đơn tuần hiện tại
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                var today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                fromDate = today.AddDays(-diff).Date;
                toDate = fromDate.Value.AddDays(6).Date;
            }

            var model = await _shiftChangeService.GetShiftChangeListAsync(
                empId, empName, empRole, fromDate, toDate, status ?? string.Empty);

            return View(model);
        }

        [Authorize(Roles = "Cashier,Bartender,Busser,Barista,Staff,Employee")]
        [HttpPost]
        public async Task<IActionResult> Create(ShiftChangeCreateViewModel model)
        {
            model.EmployeeId = GetCurrentEmployeeId();
            model.EmployeeName = GetCurrentFullName();
            model.RoleName = GetCurrentRole();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin.";
                return RedirectToAction(nameof(EmployeeIndex));
            }

            var result = await _shiftChangeService.CreateShiftChangeAsync(model);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(EmployeeIndex));
        }

        [Authorize(Roles = "Cashier,Bartender,Busser,Barista,Staff,Employee")]
        [HttpPost]
        public async Task<IActionResult> Cancel(int requestId)
        {
            var empId = GetCurrentEmployeeId();
            var result = await _shiftChangeService.CancelShiftChangeAsync(requestId, empId);

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(EmployeeIndex));
        }
    }

    // ViewModel dành riêng cho màn hình quản lý (tránh conflict với ViewModel nhân viên)
    public class ManagerShiftChangeListViewModel
    {
        public List<ShiftChangeRequest> Requests { get; set; } = new();
        public List<Employee> EmployeeOptions { get; set; } = new();
        public int? SelectedEmployeeId { get; set; }
        public string? SelectedStatus { get; set; }
        public string? Keyword { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class ShiftDecisionRequest
    {
        public int RequestId { get; set; }
    }
}