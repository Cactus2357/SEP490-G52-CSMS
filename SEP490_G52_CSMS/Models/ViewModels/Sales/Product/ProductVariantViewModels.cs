using System.ComponentModel.DataAnnotations;

namespace SEP490_G52_CSMS.Models.ViewModels.Sales.Product
{
    public class VariantIndexViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SearchString { get; set; }
        public List<VariantItemViewModel> Variants { get; set; } = new();
    }

    public class VariantItemViewModel
    {
        public int VariantId { get; set; }
        public string? SizeVariant { get; set; }
        public decimal SellingPrice { get; set; }
    }

    public class CreateVariantViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Tên biến thể không được để trống")]
        [StringLength(20)]
        [Display(Name = "Tên biến thể")]
        public string SizeVariant { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá bán")]
        public decimal SellingPrice { get; set; }
    }

    public class UpdateVariantViewModel
    {
        [Required]
        public int VariantId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Tên biến thể không được để trống")]
        [StringLength(20)]
        [Display(Name = "Tên biến thể")]
        public string SizeVariant { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá bán")]
        public decimal SellingPrice { get; set; }
    }
}
