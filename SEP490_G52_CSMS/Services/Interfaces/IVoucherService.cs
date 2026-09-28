using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<List<Voucher>> GetBranchVouchersAsync(string branchId, string? status = null, string? search = null);
        Task<Voucher?> GetVoucherByIdAsync(int voucherId, string? branchId = null);
        Task<Voucher?> GetVoucherByCodeAsync(string voucherCode, string? branchId = null);
        Task<(bool Success, string Message, Voucher? Voucher, decimal DiscountPercent, decimal DiscountAmount)> ValidateVoucherForOrderAsync(string voucherCode, string branchId, decimal subtotal);
        Task<(bool Success, string Message)> CreateVoucherAsync(Voucher voucher, int managerId);
        Task<bool> ToggleVoucherStatusAsync(int voucherId, string branchId);
        Task<List<Order>> GetVoucherOrderHistoryAsync(int voucherId, string branchId);
        Task<bool> IncrementVoucherUsageAsync(int voucherId);
    }
}
