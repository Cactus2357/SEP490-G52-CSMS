using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface ICashHandoverRepository
    {
        /// <summary> Lấy ca đang Active của thu ngân theo ngày </summary>
        Task<CashHandover?> GetActiveHandoverAsync(int cashierId, DateTime date);

        /// <summary> Lấy ca đã đóng gần nhất của chi nhánh để hiển thị thông tin ca trước </summary>
        Task<CashHandover?> GetLastClosedHandoverForBranchAsync(string branchId);

        /// <summary> Lấy lịch trực hiện tại của nhân viên (theo ngày) </summary>
        Task<WeeklyRosterGrid?> GetCurrentRosterAsync(int cashierId, DateTime date);

        /// <summary> Tìm ID thu ngân đang trực hoặc sắp trực (nếu chưa có ai mở ca) </summary>
        Task<int?> GetCurrentCashierIdAsync(string branchId, DateTime date, TimeSpan time);

        /// <summary> Tìm người nhận ca tiếp theo dựa vào lịch làm việc </summary>
        Task<int?> GetNextCashierForHandoverAsync(string branchId, DateTime date, TimeSpan currentTime);

        /// <summary> Lấy danh sách thu ngân đang hoạt động tại chi nhánh (để chọn người nhận ca) </summary>
        Task<List<Employee>> GetCashiersInBranchAsync(string branchId);

        /// <summary> Lấy danh sách thu ngân hợp lệ có thể nhận bàn giao ca (bản thân hoặc ca tiếp theo trong ngày) </summary>
        Task<List<Employee>> GetEligibleHandoverCashiersAsync(string branchId, DateTime date, int currentShiftId, int outgoingCashierId);

        /// <summary> Lấy nhân viên theo ID (để verify mật khẩu) </summary>
        Task<Employee?> GetEmployeeByIdAsync(int employeeId);

        /// <summary> Tạo mới bản ghi CashHandover (mở ca) </summary>
        Task AddHandoverAsync(CashHandover handover);

        /// <summary> Cập nhật bản ghi CashHandover (đóng ca) </summary>
        Task UpdateHandoverAsync(CashHandover handover);

        /// <summary> Lấy danh sách lịch sử giao ca của chi nhánh (phân trang) </summary>
        Task<List<CashHandover>> GetHandoverHistoryAsync(string branchId, int pageIndex, int pageSize);

        /// <summary> Đếm tổng số bản ghi giao ca của chi nhánh </summary>
        Task<int> GetHandoverCountAsync(string branchId);

        /// <summary> Lấy CashHandover theo ID </summary>
        Task<CashHandover?> GetHandoverByIdAsync(int handoverId);

        /// <summary> Lấy toàn bộ danh sách ca cố định </summary>
        Task<List<FixedShift>> GetAllFixedShiftsAsync();

        /// <summary> Xác định giai đoạn của ca: FirstShift (Đầu ngày), MidShift (Giữa ngày), LastShift (Cuối ngày) </summary>
        Task<string> GetShiftPhaseAsync(int shiftId, string? branchId = null, DateTime? date = null);

        /// <summary> Lấy ca cố định tiếp theo </summary>
        Task<FixedShift?> GetNextFixedShiftAsync(int currentShiftId);

        /// <summary> Tính doanh thu tiền mặt, CK và hoàn tiền mặt phát sinh trong ca </summary>
        Task<(decimal cashRevenue, decimal bankRevenue, decimal cashRefunds)> GetShiftSalesStatsAsync(string branchId, int? cashierId, DateTime openedAt, DateTime? closedAt);

        /// <summary> Kiểm tra điều kiện mở bán hàng cho Thu ngân (2 điều kiện) </summary>
        Task<(bool isEligible, string reasonCode, string message, int? activeShiftId, string? shiftPhase)> CheckCashierSaleEligibilityAsync(int cashierId, string branchId);

        /// <summary> Lấy phân công trực cho ca cụ thể </summary>
        Task<WeeklyRosterGrid?> GetRosterForShiftAsync(string branchId, int shiftId, DateTime date);

        /// <summary> Lấy ca Active hiện tại của chi nhánh </summary>
        Task<CashHandover?> GetCurrentActiveHandoverForBranchAsync(string branchId, DateTime date);

        /// <summary> Kiểm tra xem ca làm việc cụ thể tại chi nhánh đã được mở hôm nay chưa </summary>
        Task<CashHandover?> GetActiveHandoverByShiftAsync(string branchId, int shiftId, DateTime date);

        /// <summary> Đồng bộ dữ liệu chấm công khi mở ca </summary>
        Task SyncAttendanceOnOpenShiftAsync(int employeeId, int shiftId, DateTime date);

        /// <summary> Đồng bộ dữ liệu chấm công khi đóng/giao ca </summary>
        Task SyncAttendanceOnCloseShiftAsync(int employeeId, int shiftId, DateTime date);

        /// <summary> Kiểm tra xem ngày hôm nay tại chi nhánh đã thực hiện Đóng ca cuối ngày (HandoverTypeLastShift) chưa </summary>
        Task<bool> IsDayClosedAsync(string branchId, DateTime date);

        /// <summary> Kiểm tra xem ca làm việc cụ thể tại chi nhánh đã bị đóng (Closed) chưa </summary>
        Task<CashHandover?> GetClosedHandoverByShiftAsync(string branchId, int shiftId, DateTime date);

        /// <summary> Thêm đơn xin đổi ca tự động khi bàn giao ca đột xuất </summary>
        Task AddShiftChangeRequestAsync(ShiftChangeRequest request);
    }
}
