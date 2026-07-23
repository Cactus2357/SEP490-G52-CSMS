using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Services;

namespace SEP490_G52_CSMS.Controllers
{
    public class WorkScheduleController : Controller
    {
        private readonly IWorkScheduleService _scheduleService;

        public WorkScheduleController(IWorkScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        // =========================================================
        //  TRANG CHÍNH — HIỂN THỊ LỊCH LÀM VIỆC
        // =========================================================
        public async Task<IActionResult> Index(int employeeId = 0, string weekDate = "")
        {
            if (employeeId <= 0)
            {
                // Fake auth: get the first employee as the logged-in user
                var fakeUser = await _scheduleService.GetEmployeesForSelectionAsync("CN001");
                if (fakeUser.Any())
                {
                    employeeId = fakeUser.First().EmployeeId;
                }
                else
                {
                    return RedirectToAction(nameof(SelectEmployee));
                }
            }

            DateTime dateToView = DateTime.Today;
            if (!string.IsNullOrEmpty(weekDate))
            {
                if (weekDate.Contains("-W"))
                {
                    var parts = weekDate.Split("-W");
                    if (parts.Length == 2 && int.TryParse(parts[0], out int year) && int.TryParse(parts[1], out int week))
                    {
                        dateToView = System.Globalization.ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
                    }
                }
                else if (DateTime.TryParse(weekDate, out DateTime parsedDate))
                {
                    dateToView = parsedDate;
                }
            }

            var model = await _scheduleService.GetEmployeeScheduleAsync(employeeId, dateToView);
            
            if (model == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên.";
                return RedirectToAction(nameof(SelectEmployee));
            }

            return View(model);
        }

        // =========================================================
        //  CHỌN NHÂN VIÊN
        // =========================================================
        public async Task<IActionResult> SelectEmployee(string branchId = "CN001")
        {
            var employees = await _scheduleService.GetEmployeesForSelectionAsync(branchId);
            ViewBag.BranchId = branchId;
            return View(employees);
        }
    }
}
