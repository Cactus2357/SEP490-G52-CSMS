namespace SEP490_G52_CSMS.Commons.Constants
{
    public static class CashHandoverConstants
    {
        // Trạng thái ca làm việc
        public const string ActiveStatus = "Active";
        public const string ClosedStatus = "Closed";

        // Role thu ngân
        public const string CashierRole = "Cashier";

        // Nhãn hiển thị
        public const string UnassignedCashierLabel = "Chưa xác định";
        public const string NoHistoryLabel = "Chưa có lịch sử giao ca";

        // Default shift
        public const int DefaultShiftId = 1;

        // Số bản ghi mỗi trang lịch sử
        public const int HistoryPageSize = 10;
    }
}
