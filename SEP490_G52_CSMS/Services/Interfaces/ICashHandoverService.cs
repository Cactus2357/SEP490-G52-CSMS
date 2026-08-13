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

        /// <summary> Lấy dữ liệu cho form Giao & Đóng Ca </summary>
        Task<HandoverViewModel?> GetHandoverModelAsync(int cashierId);

        /// <summary> Thực hiện giao ca: xác minh mật khẩu, đóng ca, lưu đối soát </summary>
        Task<OperationResult> CloseShiftAsync(HandoverViewModel model);

        /// <summary> Lấy danh sách lịch sử giao ca của chi nhánh </summary>
        Task<HandoverHistoryViewModel> GetHistoryAsync(string branchId, int pageIndex);

        /// <summary> Lấy danh sách thu ngân Active của chi nhánh để chọn </summary>
        Task<List<CashierSelectItemViewModel>> GetCashiersAsync(string branchId);

        /// <summary>
        /// Tìm ID thu ngân đang trực hoặc sắp trực ở hiện tại
        /// </summary>
        Task<int?> GetCurrentCashierIdAsync(string branchId);
    }
}
