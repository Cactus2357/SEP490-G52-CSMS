using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Core
{
    /// <summary>
    /// BẢNG CẤU HÌNH THÀNH PHẦN VÀ CÀI ĐẶT CHUNG CỦA CHI NHÁNH
    /// </summary>
    [Table("branch_settings")]
    public class BranchSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("setting_id")]
        public int SettingId { get; set; }

        [Column("branch_id")]
        [StringLength(50)]
        public string BranchId { get; set; } = "CB004";

        // General Branch Settings
        [Column("branch_display_name")]
        [StringLength(100)]
        public string BranchDisplayName { get; set; } = "CSMS Coffee";

        [Column("contact_phone")]
        [StringLength(20)]
        public string ContactPhone { get; set; } = "0988888888";

        [Column("address")]
        [StringLength(255)]
        public string Address { get; set; } = "";

        [Column("opening_hours")]
        [StringLength(100)]
        public string OpeningHours { get; set; } = "06:30 - 22:30";

        [Column("auto_print_receipt")]
        public bool AutoPrintReceipt { get; set; } = true;

        [Column("enable_sound_notification")]
        public bool EnableSoundNotification { get; set; } = true;

        // SePay & Bank Account Settings
        [Column("bank_code")]
        [StringLength(20)]
        public string BankCode { get; set; } = "MBBank";

        [Column("account_number")]
        [StringLength(50)]
        public string AccountNumber { get; set; } = "0333333333";

        [Column("account_name")]
        [StringLength(100)]
        public string AccountName { get; set; } = "CSMS CAFE";

        [Column("sepay_api_key")]
        [StringLength(200)]
        public string? SePayApiKey { get; set; } = "";

        [Column("webhook_secret_token")]
        [StringLength(200)]
        public string? WebhookSecretToken { get; set; } = "";

        [Column("transfer_prefix")]
        [StringLength(50)]
        public string TransferPrefix { get; set; } = "CSMS";

        [Column("auto_confirm_order")]
        public bool AutoConfirmOrder { get; set; } = true;

        [Column("is_sepay_active")]
        public bool IsSePayActive { get; set; } = true;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }
    }
}
