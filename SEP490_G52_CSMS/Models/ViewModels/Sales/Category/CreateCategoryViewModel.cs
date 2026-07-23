using System.ComponentModel.DataAnnotations;

namespace SEP490_G52_CSMS.Models.ViewModels.Sales.Category
{
    public class CreateCategoryViewModel
    {
        [Required]
        [StringLength(150)]
        public string CategoryName { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}