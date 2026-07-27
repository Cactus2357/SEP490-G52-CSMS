using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Services;

namespace SEP490_G52_CSMS.Controllers
{
    public class WorkScheduleController : Controller
    {
        private const string DefaultBranchId = "CB001";

        private readonly IWeeklyRosterService _service;
        private readonly CSMSAppDbContext _context;

        public WorkScheduleController(IWeeklyRosterService service, CSMSAppDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? weekStart)
        {
            var monday = GetMondayOfWeek(weekStart ?? DateTime.Today);
            var sunday = monday.AddDays(6);

            var shifts = await _context.FixedShifts
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var rosterEntries = await _context.WeeklyRosterGrids
                .Where(r => r.BranchId == DefaultBranchId
                            && r.AssignmentDate >= monday
                            && r.AssignmentDate <= sunday)
                .Include(r => r.Employee)
                .Include(r => r.FixedShift)
                .ToListAsync();

            var vm = new WeeklyScheduleViewModel
            {
                WeekStart = monday,
                WeekEnd = sunday,
                Shifts = shifts,
                Days = Enumerable.Range(0, 7).Select(i => monday.AddDays(i)).ToList(),
                Assignments = rosterEntries
                    .Where(r => r.Employee != null)
                    .GroupBy(r => (r.ShiftId, r.AssignmentDate.Date))
                    .ToDictionary(g => g.Key, g => g.Select(r => r.Employee!).ToList())
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Manage(DateTime? weekStart)
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

        [HttpPost]
        public async Task<IActionResult> UpdateWorkSchedule([FromBody] CreateRosterVM vm)
        {
            if (vm == null || string.IsNullOrWhiteSpace(vm.BranchId))
            {
                return BadRequest("Invalid request.");
            }

            string result = await _service.UpdateAsync(vm);

            if (result != "Success")
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? weekStart)
        {
            var monday = GetMondayOfWeek(weekStart ?? DateTime.Today);
            var sunday = monday.AddDays(6);

            var rosterEntries = await _context.WeeklyRosterGrids
                .Where(r => r.BranchId == DefaultBranchId
                            && r.AssignmentDate >= monday
                            && r.AssignmentDate <= sunday)
                .Include(r => r.Employee)
                .Include(r => r.FixedShift)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Ca,Ngay,NhanVien,VaiTro");
            foreach (var r in rosterEntries)
            {
                csv.AppendLine($"{r.FixedShift?.ShiftName},{r.AssignmentDate:dd/MM/yyyy},{r.Employee?.FullName},{r.Employee?.Role}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"LichLamViec_{monday:ddMM}_{sunday:ddMM}.csv");
        }

        private static DateTime GetMondayOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.Date.AddDays(-diff);
        }

    }

    public class WeeklyScheduleViewModel
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<FixedShift> Shifts { get; set; } = new();
        public List<DateTime> Days { get; set; } = new();
        public Dictionary<(int ShiftId, DateTime Date), List<Employee>> Assignments { get; set; } = new();
    }
}