namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class CashierEligibilityResult
    {
        public bool IsEligible { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? ActiveShiftId { get; set; }
        public string? ShiftName { get; set; }
        public string? ShiftPhase { get; set; }
        public bool IsCheckedIn { get; set; }
        public bool IsShiftOpened { get; set; }
    }
}
