using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services;

namespace SEP490_G52_CSMS.Controllers
{
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly CSMSAppDbContext _context;

        public LeaveRequestController(ILeaveRequestService leaveRequestService, CSMSAppDbContext context)
        {
            _leaveRequestService = leaveRequestService;
            _context = context;
        }

        private async Task<(int Id, string Name, string Role)> GetMockCurrentUserAsync()
        {
            var emp = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                System.Linq.Queryable.OrderBy(
                    System.Linq.Queryable.Where(_context.Employees, e => e.BranchId == "CN001"),
                    e => e.FullName
                )
            );
            if (emp != null)
                return (emp.EmployeeId, emp.FullName ?? emp.Username, emp.Role ?? "Thu ngân");
            return (4, "Nguyễn Văn A", "Thu ngân");
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string status)
        {
            var currentUser = await GetMockCurrentUserAsync();
            // Mặc định load đơn tuần này nếu không chọn ngày (theo business rule)
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
        public async Task<IActionResult> Create(LeaveRequestCreateViewModel model)
        {
            var currentUser = await GetMockCurrentUserAsync();
            model.EmployeeId = currentUser.Id;
            model.EmployeeName = currentUser.Name;
            // model.RoleName is mapped from the form POST (dropdown)

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
        public async Task<IActionResult> Cancel(int applicationId)
        {
            var currentUser = await GetMockCurrentUserAsync();
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
    }
}
