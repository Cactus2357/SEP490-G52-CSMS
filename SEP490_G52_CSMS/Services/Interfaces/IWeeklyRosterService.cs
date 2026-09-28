namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IWeeklyRosterService
    {
        Task<string> CreateAsync(CreateRosterVM vm);
        Task<string> UpdateAsync(CreateRosterVM vm);
        Task<string> UpsertAsync(CreateRosterVM vm);
        Task<AddWorkScheduleVM> GetFormOptionsAsync(string branchId, DateTime weekStartDate);
    }
}