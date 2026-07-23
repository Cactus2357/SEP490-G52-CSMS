namespace SEP490_G52_CSMS.Models.ViewModels
{
    /// <summary>
    /// Thông tin thu ngân hiển thị trong trang chọn thu ngân trước khi mở ca
    /// </summary>
    public class CashierSelectItemViewModel
    {
        public int CashierId { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        /// <summary> Có ca đang mở hôm nay không </summary>
        public bool HasActiveShift { get; set; }
        /// <summary> Tên ca đang mở (nếu có) </summary>
        public string ActiveShiftName { get; set; } = string.Empty;
    }
}
