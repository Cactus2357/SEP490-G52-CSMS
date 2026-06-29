using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Reponsitories;

namespace SEP490_G52_CSMS.Services
{
    public class CreateRosterVM
    {
        public string BranchId { get; set; }

        public DateTime AssignmentDate { get; set; }

        public int ShiftId { get; set; }

        public List<int> EmployeeIds { get; set; } = new();
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
    }
}
