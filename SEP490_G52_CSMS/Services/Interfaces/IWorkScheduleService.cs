using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IWorkScheduleService
    {
        Task<List<SelectEmployeeScheduleViewModel>> GetEmployeesForSelectionAsync(string branchId);
        Task<WorkScheduleViewModel?> GetEmployeeScheduleAsync(int employeeId, DateTime dateInWeek);
    }
}
