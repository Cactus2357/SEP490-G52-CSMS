using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SEP490_G52_CSMS.Models.ViewModels.Sales.Product
{
    public class UpdateProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(
            255,
            ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục là bắt buộc.")]
        public int CategoryId { get; set; }

        [StringLength(
            500,
            ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ExistingImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string Status { get; set; } = "Active";

        public List<SelectListItem> Categories { get; set; }
            = new List<SelectListItem>();
    }
}