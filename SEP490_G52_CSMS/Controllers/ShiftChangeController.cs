using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services;

namespace SEP490_G52_CSMS.Controllers
{
    public class ShiftChangeController : Controller
    {
        private readonly IShiftChangeService _shiftChangeService;
        private readonly CSMSAppDbContext _context;

        public ShiftChangeController(IShiftChangeService shiftChangeService, CSMSAppDbContext context)
        {
            _shiftChangeService = shiftChangeService;
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
            // Mặc định load đơn tuần này nếu không chọn ngày
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                var today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                fromDate = today.AddDays(-1 * diff).Date;
                toDate = fromDate.Value.AddDays(6).Date;
            }

            var model = await _shiftChangeService.GetShiftChangeListAsync(currentUser.Id, currentUser.Name, currentUser.Role, fromDate, toDate, status);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ShiftChangeCreateViewModel model)
        {
            var currentUser = await GetMockCurrentUserAsync();
            model.EmployeeId = currentUser.Id;
            model.EmployeeName = currentUser.Name;
            model.RoleName = currentUser.Role;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _shiftChangeService.CreateShiftChangeAsync(model);
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
        public async Task<IActionResult> Cancel(int requestId)
        {
            var currentUser = await GetMockCurrentUserAsync();
            var result = await _shiftChangeService.CancelShiftChangeAsync(requestId, currentUser.Id);
            
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
