using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services
{
    public interface IWorkScheduleService
    {
        Task<List<SelectEmployeeScheduleViewModel>> GetEmployeesForSelectionAsync(string branchId);
        Task<WorkScheduleViewModel?> GetEmployeeScheduleAsync(int employeeId, DateTime dateInWeek);
    }
}
