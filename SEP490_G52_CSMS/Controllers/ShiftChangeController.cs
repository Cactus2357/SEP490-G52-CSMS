using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Controllers
{
    public class ShiftChangeController : Controller
    {
        // TODO: replace with the branch of the currently authenticated B Manager
        // once auth/session context is wired up (same placeholder pattern as WorkScheduleController).
        private const string DefaultBranchId = "CB001";
        private const int PageSize = 8;

        private readonly CSMSAppDbContext _context;

        public ShiftChangeController(CSMSAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? employeeId, string? status, int page = 1)
        {
            IQueryable<ShiftChangeRequest> query = _context.ShiftChangeRequests
                .Include(r => r.RequestingEmployee)
                .Where(r => r.RequestingEmployee != null && r.RequestingEmployee.BranchId == DefaultBranchId);

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
                .Where(e => e.BranchId == DefaultBranchId && e.Role != "BranchManager")
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
            var request = await _context.ShiftChangeRequests
                .Include(r => r.RequestingEmployee)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound();
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
                .FirstOrDefaultAsync(r => r.RequestId == req.RequestId);

            if (request == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy đơn yêu cầu." });
            }

            if (request.Status != "Submitted")
            {
                return Json(new { success = false, errorMessage = "Đơn này đã được xử lý trước đó." });
            }

            request.Status = "Approved";
            request.ApprovedBranchId = DefaultBranchId;
            // TODO: set request.ApprovedManagerId from the authenticated B Manager's EmployeeId.

            // TODO (BR03): "Programmatically recalculates and updates the corresponding branch
            // weekly shift configuration grid with the new employee assignment information."
            // ShiftChangeRequest currently only stores a free-text "Aspiration" field (no
            // structured OldShiftId/NewShiftId/AssignmentDate columns), so there isn't enough
            // structured data here to safely locate and rewrite a WeeklyRosterGrid row.
            // Once the schema exposes those columns, look up the matching WeeklyRosterGrid
            // row (EmployeeId + old shift/date) and move it to the new shift/date here.

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
                .FirstOrDefaultAsync(r => r.RequestId == req.RequestId);

            if (request == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy đơn yêu cầu." });
            }

            if (request.Status != "Submitted")
            {
                return Json(new { success = false, errorMessage = "Đơn này đã được xử lý trước đó." });
            }

            request.Status = "Rejected";
            request.ApprovedBranchId = DefaultBranchId;
            // TODO: set request.ApprovedManagerId from the authenticated B Manager's EmployeeId.

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