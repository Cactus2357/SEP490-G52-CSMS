using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class CreateRosterVM
    {
        public string BranchId { get; set; }

        public DateTime AssignmentDate { get; set; }

        public int ShiftId { get; set; }

        public List<int> EmployeeIds { get; set; } = new();
    }

    public class ExistingAssignmentVM
    {
        // 0 = Monday ... 6 = Sunday, relative to the week being viewed.
        public int DayOffset { get; set; }

        public int ShiftId { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public string? Username { get; set; }

        public string Role { get; set; }

        public bool IsCheckedIn { get; set; }
    }

    public class AddWorkScheduleVM
    {
        public string BranchId { get; set; }

        public DateTime WeekStartDate { get; set; }

        public List<FixedShift> Shifts { get; set; } = new();

        public List<Employee> Cashiers { get; set; } = new();

        public List<Employee> Bartenders { get; set; } = new();

        public List<Employee> Bussers { get; set; } = new();

        public List<ExistingAssignmentVM> ExistingAssignments { get; set; } = new();
    }

    public class WeeklyRosterService : IWeeklyRosterService
    {
        private readonly IWeeklyRosterRepository _repository;

        public WeeklyRosterService(IWeeklyRosterRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> CreateAsync(CreateRosterVM vm)
        {
            return await UpsertAsync(vm);
        }

        public async Task<string> UpdateAsync(CreateRosterVM vm)
        {
            return await UpsertAsync(vm);
        }

        public async Task<string> UpsertAsync(CreateRosterVM vm)
        {
            if (vm == null || string.IsNullOrWhiteSpace(vm.BranchId))
                return "Yêu cầu không hợp lệ.";

            var existingRosters = await _repository.GetRosterForShiftAsync(vm.BranchId, vm.AssignmentDate, vm.ShiftId);

            // Case 1: Empty or cleared employee list -> remove existing if any
            if (vm.EmployeeIds == null || vm.EmployeeIds.Count == 0)
            {
                if (existingRosters.Count > 0)
                {
                    var checkedInRosters = existingRosters.Where(r => r.AttendanceLogs != null &&
                        r.AttendanceLogs.Any(a => a.CheckInTime != null || a.OverallStatus == "Present" || a.CheckInStatus == "OnTime" || a.CheckInStatus == "Late")).ToList();

                    if (checkedInRosters.Count > 0)
                    {
                        var names = string.Join(", ", checkedInRosters.Select(r => r.Employee?.FullName ?? $"NV#{r.EmployeeId}"));
                        var shiftName = checkedInRosters.First().FixedShift?.ShiftName ?? $"Ca #{vm.ShiftId}";
                        return $"Không thể xóa ca làm việc ngày {vm.AssignmentDate:dd/MM/yyyy} ({shiftName}) vì nhân viên ({names}) đã điểm danh vào ca.";
                    }

                    // Remove dummy/orphan attendance logs if any before deleting rosters to avoid FK conflict
                    var dummyLogs = existingRosters.SelectMany(r => r.AttendanceLogs ?? Enumerable.Empty<AttendanceLog>())
                        .Where(a => a.CheckInTime == null && a.OverallStatus != "Present")
                        .ToList();
                    if (dummyLogs.Count > 0)
                    {
                        _repository.RemoveAttendanceLogs(dummyLogs);
                    }

                    _repository.RemoveRange(existingRosters);
                    await _repository.SaveAsync();
                }
                return "Success";
            }

            var newEmployeeIds = vm.EmployeeIds.Distinct().ToList();

            // Case 2: Shift does not exist yet -> Create new rosters
            if (existingRosters.Count == 0)
            {
                var rostersToAdd = newEmployeeIds.Select(id => new WeeklyRosterGrid
                {
                    BranchId = vm.BranchId,
                    AssignmentDate = vm.AssignmentDate.Date,
                    ShiftId = vm.ShiftId,
                    EmployeeId = id
                }).ToList();

                await _repository.AddRangeAsync(rostersToAdd);
                await _repository.SaveAsync();
                return "Success";
            }

            // Case 3: Shift already exists -> Update / Synchronize
            var existingEmployeeIds = existingRosters.Select(r => r.EmployeeId).ToList();
            var toRemove = existingRosters.Where(r => !newEmployeeIds.Contains(r.EmployeeId)).ToList();
            var toAddIds = newEmployeeIds.Where(id => !existingEmployeeIds.Contains(id)).ToList();

            if (toRemove.Count > 0)
            {
                var checkedInToRemove = toRemove.Where(r => r.AttendanceLogs != null &&
                    r.AttendanceLogs.Any(a => a.CheckInTime != null || a.OverallStatus == "Present" || a.CheckInStatus == "OnTime" || a.CheckInStatus == "Late")).ToList();

                if (checkedInToRemove.Count > 0)
                {
                    var names = string.Join(", ", checkedInToRemove.Select(r => r.Employee?.FullName ?? $"NV#{r.EmployeeId}"));
                    var shiftName = checkedInToRemove.First().FixedShift?.ShiftName ?? $"Ca #{vm.ShiftId}";
                    return $"Không thể xóa hoặc thay thế nhân viên ({names}) khỏi ca {shiftName} ngày {vm.AssignmentDate:dd/MM/yyyy} do nhân viên đã điểm danh vào ca làm việc.";
                }

                // Clean up dummy/orphan attendance logs without check-in before removing rosters
                var dummyLogs = toRemove.SelectMany(r => r.AttendanceLogs ?? Enumerable.Empty<AttendanceLog>())
                    .Where(a => a.CheckInTime == null && a.OverallStatus != "Present")
                    .ToList();
                if (dummyLogs.Count > 0)
                {
                    _repository.RemoveAttendanceLogs(dummyLogs);
                }

                _repository.RemoveRange(toRemove);
            }

            if (toAddIds.Count > 0)
            {
                var rostersToAdd = toAddIds.Select(id => new WeeklyRosterGrid
                {
                    BranchId = vm.BranchId,
                    AssignmentDate = vm.AssignmentDate.Date,
                    ShiftId = vm.ShiftId,
                    EmployeeId = id
                }).ToList();
                await _repository.AddRangeAsync(rostersToAdd);
            }

            await _repository.SaveAsync();

            return "Success";
        }

        public async Task<AddWorkScheduleVM> GetFormOptionsAsync(string branchId, DateTime weekStartDate)
        {
            var shifts = await _repository.GetAllShiftsAsync();
            var cashiers = await _repository.GetEmployeesByRoleAsync(branchId, "Cashier");
            var bartenders = await _repository.GetEmployeesByRoleAsync(branchId, "Bartender");
            var bussers = await _repository.GetEmployeesByRoleAsync(branchId, "Busser");

            var weekEnd = weekStartDate.Date.AddDays(6);
            var existingRows = await _repository.GetRosterForWeekAsync(branchId, weekStartDate, weekEnd);

            var existingAssignments = existingRows
                .Select(row => new ExistingAssignmentVM
                {
                    DayOffset = (row.AssignmentDate.Date - weekStartDate.Date).Days,
                    ShiftId = row.ShiftId,
                    EmployeeId = row.EmployeeId,
                    EmployeeName = row.Employee?.FullName ?? $"NV#{row.EmployeeId}",
                    Username = row.Employee?.Username,
                    Role = row.Employee?.Role ?? "",
                    IsCheckedIn = row.AttendanceLogs != null && row.AttendanceLogs.Any(a => a.CheckInTime != null || a.OverallStatus == "Present" || a.CheckInStatus == "OnTime" || a.CheckInStatus == "Late")
                })
                // Defensive: a stray row outside 0..6 (bad data / timezone edge case)
                // would break the grid's day columns, so drop it instead of crashing.
                .Where(a => a.DayOffset >= 0 && a.DayOffset <= 6)
                .ToList();

            return new AddWorkScheduleVM
            {
                BranchId = branchId,
                WeekStartDate = weekStartDate,
                Shifts = shifts,
                Cashiers = cashiers,
                Bartenders = bartenders,
                Bussers = bussers,
                ExistingAssignments = existingAssignments
            };
        }
    }
}
