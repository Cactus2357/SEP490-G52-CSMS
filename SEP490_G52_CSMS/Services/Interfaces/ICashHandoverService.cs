using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface ICashHandoverService
    {
        /// <summary> Lấy dữ liệu cho form Mở Ca </summary>
        Task<OpenShiftViewModel?> GetOpenShiftModelAsync(int cashierId);

        /// <summary> Thực hiện mở ca: khai báo tiền đầu ca và tạo bản ghi CashHandover </summary>
        Task<OperationResult> OpenShiftAsync(OpenShiftViewModel model);

        /// <summary> Lấy dữ liệu cho form Giao Ca giữa ngày </summary>
        Task<HandoverViewModel?> GetHandoverModelAsync(int cashierId);

        /// <summary> Thực hiện giao ca: xác minh mật khẩu, đóng ca, lưu đối soát </summary>
        Task<OperationResult> CloseShiftAsync(HandoverViewModel model);

        /// <summary> Lấy dữ liệu cho form Đóng ca cuối ngày </summary>
        Task<CloseShiftViewModel?> GetCloseShiftModelAsync(int cashierId);

        /// <summary> Thực hiện đóng ca cuối ngày (chốt sổ ngày) </summary>
        Task<OperationResult> CloseShiftOfDayAsync(CloseShiftViewModel model);

        /// <summary> Lấy dữ liệu cho form Bàn giao đột xuất giữa ca </summary>
        Task<HandoverViewModel?> GetEmergencyHandoverModelAsync(int cashierId);

        /// <summary> Thực hiện bàn giao đột xuất giữa ca (ốm/việc gấp) </summary>
        Task<OperationResult> EmergencyHandoverAsync(HandoverViewModel model);

        /// <summary> Lấy danh sách lịch sử giao ca của chi nhánh </summary>
        Task<HandoverHistoryViewModel> GetHistoryAsync(string branchId, int pageIndex);

        /// <summary> Lấy danh sách thu ngân Active của chi nhánh để chọn </summary>
        Task<List<CashierSelectItemViewModel>> GetCashiersAsync(string branchId);

        /// <summary>
        /// Tìm ID thu ngân đang trực hoặc sắp trực ở hiện tại
        /// </summary>
        Task<int?> GetCurrentCashierIdAsync(string branchId);

        /// <summary> Kiểm tra điều kiện mở bán hàng cho Thu ngân </summary>
        Task<(bool isEligible, string reasonCode, string message, int? activeShiftId, string? shiftPhase)> CheckCashierSaleEligibilityAsync(int cashierId, string branchId);

        /// <summary> Xác định loại ca hiện tại: FirstShift, MidShift, LastShift </summary>
        Task<string> DetermineCurrentShiftPhaseAsync(int cashierId, string branchId);
    }
}
