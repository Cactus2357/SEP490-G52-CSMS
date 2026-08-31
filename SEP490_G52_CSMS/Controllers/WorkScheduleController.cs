using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize]
    public class WorkScheduleController : Controller
    {
        private readonly IWeeklyRosterService _service;
        private readonly CSMSAppDbContext _context;

        public WorkScheduleController(IWeeklyRosterService service, CSMSAppDbContext context)
        {
            _service = service;
            _context = context;
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
            return "";
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? weekStart)
        {
            if (!User.IsInRole("BranchManager") && !User.IsInRole("RManager"))
            {
                return RedirectToAction(nameof(EmployeeIndex), new { weekStart });
            }

            var loggedInBranchId = await GetUserBranchIdAsync();
            var monday = GetMondayOfWeek(weekStart ?? DateTime.Today);
            var sunday = monday.AddDays(6);

            var shifts = await _context.FixedShifts
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var rosterEntries = await _context.WeeklyRosterGrids
                .Where(r => r.BranchId == loggedInBranchId
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
        public async Task<IActionResult> EmployeeIndex(DateTime? weekStart)
        {
            var empIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(empIdStr, out int empId))
            {
                return Unauthorized();
            }

            var employee = await _context.Employees.FindAsync(empId);
            if (employee == null) return NotFound();

            var monday = GetMondayOfWeek(weekStart ?? DateTime.Today);
            var sunday = monday.AddDays(6);

            var shifts = await _context.FixedShifts
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var rosters = await _context.WeeklyRosterGrids
                .Include(r => r.AttendanceLogs)
                .Where(r => r.EmployeeId == empId && r.AssignmentDate >= monday && r.AssignmentDate <= sunday)
                .ToListAsync();

            var scheduleData = new Dictionary<(int, DateTime), EmployeeShiftDetail>();
            foreach (var r in rosters)
            {
                var key = (r.ShiftId, r.AssignmentDate.Date);
                var log = r.AttendanceLogs.FirstOrDefault();
                var detail = new EmployeeShiftDetail
                {
                    IsAssigned = true,
                    OverallStatus = log?.OverallStatus,
                    CheckInTime = log?.CheckInTime,
                    CheckOutTime = log?.CheckOutTime
                };

                if (!scheduleData.ContainsKey(key))
                {
                    scheduleData[key] = detail;
                }
                else
                {
                    if (log != null && (log.CheckInTime != null || !string.IsNullOrEmpty(log.OverallStatus)))
                    {
                        scheduleData[key] = detail;
                    }
                }
            }

            var vm = new EmployeeScheduleViewModel
            {
                FullName = employee.FullName ?? string.Empty,
                WeekStart = monday,
                WeekEnd = sunday,
                Shifts = shifts,
                Days = Enumerable.Range(0, 7).Select(i => monday.AddDays(i)).ToList(),
                ScheduleData = scheduleData
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "BranchManager")]
        public async Task<IActionResult> Manage(DateTime? weekStart)
        {
            var loggedInBranchId = await GetUserBranchIdAsync();
            var monday = GetMondayOfWeek(weekStart ?? DateTime.Today);

            var vm = await _service.GetFormOptionsAsync(loggedInBranchId, monday);

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "BranchManager")]
        public async Task<IActionResult> AddWorkSchedule([FromBody] CreateRosterVM vm)
        {
            if (vm == null || string.IsNullOrWhiteSpace(vm.BranchId))
            {
                return BadRequest("Invalid request.");
            }

            var loggedInBranchId = await GetUserBranchIdAsync();
            if (vm.BranchId != loggedInBranchId)
            {
                return Forbid();
            }

            string result = await _service.CreateAsync(vm);

            if (result != "Success")
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "BranchManager")]
        public async Task<IActionResult> UpdateWorkSchedule([FromBody] CreateRosterVM vm)
        {
            if (vm == null || string.IsNullOrWhiteSpace(vm.BranchId))
            {
                return BadRequest("Invalid request.");
            }

            var loggedInBranchId = await GetUserBranchIdAsync();
            if (vm.BranchId != loggedInBranchId)
            {
                return Forbid();
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
            var loggedInBranchId = await GetUserBranchIdAsync();
            var monday = GetMondayOfWeek(weekStart ?? DateTime.Today);
            var sunday = monday.AddDays(6);

            var rosterEntries = await _context.WeeklyRosterGrids
                .Where(r => r.BranchId == loggedInBranchId
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

        public class QuickCheckInRequest
        {
            public int ShiftId { get; set; }
            public DateTime? Date { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCheckIn([FromBody] QuickCheckInRequest request)
        {
            var empIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(empIdStr, out int empId))
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
            }

            var targetDate = (request.Date ?? DateTime.Today).Date;

            var roster = await _context.WeeklyRosterGrids
                .Include(r => r.FixedShift)
                .Include(r => r.AttendanceLogs)
                .FirstOrDefaultAsync(r => r.EmployeeId == empId 
                                          && r.ShiftId == request.ShiftId 
                                          && r.AssignmentDate.Date == targetDate);

            if (roster == null || roster.FixedShift == null)
            {
                return BadRequest(new { success = false, message = "Không tìm thấy ca làm việc được phân công tương ứng." });
            }

            var log = roster.AttendanceLogs.FirstOrDefault();
            bool currentlyCheckedIn = log != null && log.OverallStatus == "Present";

            if (currentlyCheckedIn && log != null)
            {
                // Toggle OFF -> Delete attendance log for testing
                _context.AttendanceLogs.Remove(log);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    isCheckedIn = false,
                    message = $"Đã chuyển ca {roster.FixedShift.ShiftName} về trạng thái (Chưa điểm danh)!"
                });
            }
            else
            {
                // Toggle ON -> Add / update attendance log
                var nowLocal = DateTime.Now;
                var fs = roster.FixedShift;
                var shiftStart = roster.AssignmentDate.Date + fs.StartTime;

                if (shiftStart > nowLocal)
                {
                    return BadRequest(new { success = false, message = "Chưa đến giờ ca làm việc. Bạn không thể điểm danh ca trong tương lai!" });
                }

                var graceCutoff = shiftStart.AddMinutes(15);
                var checkInStatus = (nowLocal <= graceCutoff) ? "OnTime" : "Late";

                if (log == null)
                {
                    log = new AttendanceLog
                    {
                        RosterId = roster.RosterId,
                        EmployeeId = empId,
                        CheckInTime = DateTime.UtcNow,
                        IsFaceCheckInValid = true,
                        CheckInConfidence = 100,
                        CheckInStatus = checkInStatus,
                        OverallStatus = "Present",
                        CheckOutStatus = "NotYetCheckOut"
                    };
                    _context.AttendanceLogs.Add(log);
                }
                else
                {
                    log.CheckInTime = DateTime.UtcNow;
                    log.IsFaceCheckInValid = true;
                    log.CheckInConfidence = 100;
                    log.CheckInStatus = checkInStatus;
                    log.OverallStatus = "Present";
                    log.CheckOutStatus = "NotYetCheckOut";
                    log.CheckOutTime = null;
                }

                await _context.SaveChangesAsync();

                string statusText = checkInStatus == "OnTime" ? "Đúng giờ" : "Đi trễ";
                return Ok(new
                {
                    success = true,
                    isCheckedIn = true,
                    message = $"Đã điểm danh ca {fs.ShiftName} lúc {nowLocal:HH:mm:ss} ({statusText})!",
                    checkInTime = nowLocal.ToString("HH:mm")
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> QuickCheckIn([FromBody] QuickCheckInRequest request)
        {
            return await ToggleCheckIn(request);
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