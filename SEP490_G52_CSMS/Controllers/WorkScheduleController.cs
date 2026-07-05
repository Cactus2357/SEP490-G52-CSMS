using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Services;

namespace SEP490_G52_CSMS.Controllers
{
    public class WorkScheduleController : Controller
    {
        private const string DefaultBranchId = "CB001";

        private readonly IWeeklyRosterService _service;

        public WorkScheduleController(IWeeklyRosterService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> AddWorkSchedule(DateTime? weekStart)
        {
            var monday = GetMondayOfWeek(weekStart ?? DateTime.Today);

            var vm = await _service.GetFormOptionsAsync(DefaultBranchId, monday);

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> AddWorkSchedule([FromBody] CreateRosterVM vm)
        {
            if (vm == null || string.IsNullOrWhiteSpace(vm.BranchId))
            {
                return BadRequest("Invalid request.");
            }

            string result = await _service.CreateAsync(vm);

            if (result != "Success")
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static DateTime GetMondayOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.Date.AddDays(-diff);
        }

    }
}
