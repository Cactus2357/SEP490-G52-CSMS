using System.ComponentModel.DataAnnotations;

namespace SEP490_G52_CSMS.Models.ViewModels.Sales.Product
{
    public class CreateProductViewModel
    {
        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public string Status { get; set; } = "Active";
        public IFormFile? ImageFile { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}