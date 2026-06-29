
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Reponsitories
{
    public interface IWeeklyRosterRepository
    {
        Task AddRangeAsync(IEnumerable<WeeklyRosterGrid> rosters);

        Task<bool> ExistsAsync(string branchId, DateTime assignmentDate, int shiftId);

        Task SaveAsync();
    }
}