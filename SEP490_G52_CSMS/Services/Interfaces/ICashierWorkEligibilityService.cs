using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface ICashierWorkEligibilityService
    {
        Task<CashierEligibilityResult> CheckEligibilityAsync(int cashierId, string branchId);
    }
}
