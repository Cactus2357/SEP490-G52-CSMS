using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "BranchManager")]
    public class ShiftChangeController : Controller
    {
        private const int PageSize = 8;
        private readonly CSMSAppDbContext _context;

        public ShiftChangeController(CSMSAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? employeeId, string? status, int page = 1)
        {
            var loggedInBranchId = User.GetBranchId() ?? "";

            IQueryable<ShiftChangeRequest> query = _context.ShiftChangeRequests
                .Include(r => r.RequestingEmployee)
                .Where(r => r.RequestingEmployee != null && r.RequestingEmployee.BranchId == loggedInBranchId);

            if (employeeId.HasValue)
            {
                query = query.Where(r => r.RequestingEmployeeId == employeeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
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
                .Where(e => e.BranchId == loggedInBranchId && e.Role != "BranchManager")
                .OrderBy(e => e.FullName)
                .ToListAsync();

            var vm = new ShiftChangeListViewModel
            {
                Requests = requests,
                EmployeeOptions = employeeOptions,
                SelectedEmployeeId = employeeId,
                SelectedStatus = status,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> DetailPartial(int id)
        {
            var loggedInBranchId = User.GetBranchId() ?? "";

            var request = await _context.ShiftChangeRequests
                .Include(r => r.RequestingEmployee)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound();
            }

            if (request.RequestingEmployee == null || request.RequestingEmployee.BranchId != loggedInBranchId)
            {
                return Forbid();
            }

            return PartialView("_ShiftChangeDetail", request);
        }

        [HttpPost]
        public async Task<IActionResult> Approve([FromBody] ShiftDecisionRequest req)
        {
            if (req == null)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });
            }

            var request = await _context.ShiftChangeRequests
                .Include(r => r.RequestingEmployee)
                .FirstOrDefaultAsync(r => r.RequestId == req.RequestId);

            if (request == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy đơn yêu cầu." });
            }

            var loggedInBranchId = User.GetBranchId() ?? "";
            if (request.RequestingEmployee == null || request.RequestingEmployee.BranchId != loggedInBranchId)
            {
                return Forbid();
            }

            if (request.Status != "Submitted")
            {
                return Json(new { success = false, errorMessage = "Đơn này đã được xử lý trước đó." });
            }

            request.Status = "Approved";
            request.ApprovedBranchId = loggedInBranchId;

            var managerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(managerIdClaim, out int managerId))
            {
                request.ApprovedManagerId = managerId;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Phê duyệt đơn đổi ca thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> Reject([FromBody] ShiftDecisionRequest req)
        {
            if (req == null)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });
            }

            var request = await _context.ShiftChangeRequests
                .Include(r => r.RequestingEmployee)
                .FirstOrDefaultAsync(r => r.RequestId == req.RequestId);

            if (request == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy đơn yêu cầu." });
            }

            var loggedInBranchId = User.GetBranchId() ?? "";
            if (request.RequestingEmployee == null || request.RequestingEmployee.BranchId != loggedInBranchId)
            {
                return Forbid();
            }

            if (request.Status != "Submitted")
            {
                return Json(new { success = false, errorMessage = "Đơn này đã được xử lý trước đó." });
            }

            request.Status = "Rejected";
            request.ApprovedBranchId = loggedInBranchId;

            var managerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(managerIdClaim, out int managerId))
            {
                request.ApprovedManagerId = managerId;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã từ chối đơn đổi ca." });
        }
    }

    public class ShiftChangeListViewModel
    {
        public List<ShiftChangeRequest> Requests { get; set; } = new();
        public List<Employee> EmployeeOptions { get; set; } = new();
        public int? SelectedEmployeeId { get; set; }
        public string? SelectedStatus { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class ShiftDecisionRequest
    {
        public int RequestId { get; set; }
    }
}