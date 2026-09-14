using System.ComponentModel.DataAnnotations;

namespace SEP490_G52_CSMS.Models.ViewModels
{
    public class BranchEditViewModel
    {
        public string BranchId { get; set; } = string.Empty;

        [Display(Name = "Tên chi nhánh")]
        [Required(ErrorMessage = "Tên chi nhánh là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tên chi nhánh không được quá 255 ký tự.")]
        public string BranchName { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự.")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0 (VD: 0912345678).")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự.")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email chi nhánh")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(150, ErrorMessage = "Email không được quá 150 ký tự.")]
        public string? Email { get; set; }

        [Display(Name = "Giờ mở cửa")]
        [Required(ErrorMessage = "Giờ mở cửa là bắt buộc.")]
        public TimeSpan OpeningTime { get; set; }

        [Display(Name = "Giờ đóng cửa")]
        [Required(ErrorMessage = "Giờ đóng cửa là bắt buộc.")]
        public TimeSpan ClosingTime { get; set; }

        [Display(Name = "Gán quản lý chi nhánh")]
        public int? ManagerId { get; set; }

        public string Status { get; set; } = string.Empty;

        public List<BranchManagerOption> Managers { get; set; } = new List<BranchManagerOption>();
    }
}
