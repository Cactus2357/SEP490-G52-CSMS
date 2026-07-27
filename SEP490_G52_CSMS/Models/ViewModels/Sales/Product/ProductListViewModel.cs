namespace SEP490_G52_CSMS.Models.ViewModels.Sales.Product
{
    public class ProductListViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string Variants { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        public string? Description { get; set; }
    }
}