namespace SEP490_G52_CSMS.Models.ViewModels.Sales.Category
{
    public class CategoryListViewModel
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        public string Variants { get; set; } = string.Empty;
    }
}