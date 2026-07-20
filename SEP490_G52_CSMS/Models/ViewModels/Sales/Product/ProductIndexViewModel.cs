using Microsoft.AspNetCore.Mvc.Rendering;

namespace SEP490_G52_CSMS.Models.ViewModels.Sales.Product
{
    public class ProductIndexViewModel
    {
        public List<ProductListViewModel> Products { get; set; } = new List<ProductListViewModel>();

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public string? SearchString { get; set; }
    }
}