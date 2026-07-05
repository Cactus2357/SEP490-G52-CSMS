

namespace SEP490_G52_CSMS.Services
{
    public interface IWeeklyRosterService
    {
        Task<string> CreateAsync(CreateRosterVM vm);
        Task<string> UpdateAsync(CreateRosterVM vm);
        Task<AddWorkScheduleVM> GetFormOptionsAsync(string branchId, DateTime weekStartDate);
    }
}