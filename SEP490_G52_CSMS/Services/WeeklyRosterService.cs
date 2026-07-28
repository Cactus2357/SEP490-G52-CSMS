using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Repositories;

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

        public string Role { get; set; }
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
            if (vm.EmployeeIds.Count == 0)
                return "Require at least one employee.";

            bool exists = await _repository.ExistsAsync(vm.BranchId, vm.AssignmentDate, vm.ShiftId);

            if (exists) return "This shift has already been created.";

            List<WeeklyRosterGrid> rosters = vm.EmployeeIds
                .Select(id => new WeeklyRosterGrid
                {
                    BranchId = vm.BranchId,
                    AssignmentDate = vm.AssignmentDate,
                    ShiftId = vm.ShiftId,
                    EmployeeId = id
                })
                .ToList();

            await _repository.AddRangeAsync(rosters);

            await _repository.SaveAsync();

            return "Success";
        }

        public async Task<string> UpdateAsync(CreateRosterVM vm)
        {
            if (vm.EmployeeIds.Count == 0)
                return "Require at least one employee.";

            var existingRosters = await _repository.GetRosterForShiftAsync(vm.BranchId, vm.AssignmentDate, vm.ShiftId);

            if (existingRosters.Count == 0)
            {
                return "This shift has not been created yet.";
            }

            var existingEmployeeIds = existingRosters.Select(r => r.EmployeeId).ToList();
            var newEmployeeIds = vm.EmployeeIds.Distinct().ToList();

            var toRemove = existingRosters.Where(r => !newEmployeeIds.Contains(r.EmployeeId)).ToList();
            var toAddIds = newEmployeeIds.Where(id => !existingEmployeeIds.Contains(id)).ToList();

            if (toRemove.Count > 0)
            {
                _repository.RemoveRange(toRemove);
            }

            if (toAddIds.Count > 0)
            {
                var rostersToAdd = toAddIds.Select(id => new WeeklyRosterGrid
                {
                    BranchId = vm.BranchId,
                    AssignmentDate = vm.AssignmentDate,
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
                    Role = row.Employee?.Role ?? ""
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
