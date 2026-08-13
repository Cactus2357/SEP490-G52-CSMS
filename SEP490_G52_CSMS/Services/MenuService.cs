using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<List<MenuListViewModel>> GetMenusListAsync(string branchId)
        {
            var menus = await _menuRepository.GetMenusByBranchAsync(branchId);
            return menus.Select(m => new MenuListViewModel
            {
                MenuId = m.MenuId,
                MenuName = m.MenuName ?? string.Empty,
                UpdatedAt = m.UpdatedAt,
                IsActive = m.IsActive
            }).ToList();
        }

        public async Task<MenuEditViewModel?> GetMenuForEditAsync(int menuId)
        {
            var menu = await _menuRepository.GetMenuByIdAsync(menuId);
            if (menu == null) return null;

            var model = new MenuEditViewModel
            {
                MenuId = menu.MenuId,
                MenuName = menu.MenuName ?? string.Empty,
                BranchId = menu.BranchId ?? string.Empty
            };

            // Group variants by ProductId to match mockups (showing product and its sizes/prices)
            if (menu.MenuDetails != null && menu.MenuDetails.Any())
            {
                var groupedProducts = menu.MenuDetails
                    .Where(md => md.ProductVariant != null && md.ProductVariant.MasterProduct != null)
                    .GroupBy(md => md.ProductVariant!.ProductId)
                    .ToList();

                foreach (var group in groupedProducts)
                {
                    var firstVariant = group.First().ProductVariant!;
                    var sizesAndPrices = string.Join(" ", group.Select(md => $"{md.ProductVariant!.SizeVariant}-{md.ProductVariant.SellingPrice / 1000}K"));

                    model.Products.Add(new MenuProductViewModel
                    {
                        ProductId = firstVariant.ProductId,
                        ProductName = firstVariant.MasterProduct!.ProductName ?? string.Empty,
                        CategoryName = firstVariant.MasterProduct.ProductCategory?.CategoryName ?? string.Empty,
                        SizeAndPrice = sizesAndPrices
                    });
                }
            }

            return model;
        }

        public async Task ToggleMenuStatusAsync(string branchId, int menuId)
        {
            var menu = await _menuRepository.GetMenuByIdAsync(menuId);
            if (menu == null || menu.BranchId != branchId) return;

            if (!menu.IsActive)
            {
                // Deactivate all others and activate this one (BR01, BR02)
                await _menuRepository.SetActiveMenuAsync(branchId, menuId);
            }
            else
            {
                // Just deactivate it
                menu.IsActive = false;
                await _menuRepository.UpdateMenuAsync(menu);
            }
        }

        public async Task<List<ProductSelectionViewModel>> GetMasterProductsForSelectionAsync(int? categoryId = null)
        {
            var products = await _menuRepository.GetMasterProductsAsync(categoryId);
            var result = new List<ProductSelectionViewModel>();

            foreach (var p in products)
            {
                var sizesAndPrices = string.Join(" ", p.ProductVariants.Select(v => $"{v.SizeVariant}-{v.SellingPrice / 1000}K"));
                result.Add(new ProductSelectionViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName ?? string.Empty,
                    CategoryId = p.CategoryId,
                    CategoryName = p.ProductCategory?.CategoryName ?? string.Empty,
                    SizeAndPrice = sizesAndPrices
                });
            }

            return result;
        }

        public async Task<List<ProductCategory>> GetProductCategoriesAsync()
        {
            return await _menuRepository.GetProductCategoriesAsync();
        }

        public async Task AddMenuAsync(string branchId, string menuName, List<int> productIds)
        {
            var menu = new BranchMenu
            {
                BranchId = branchId,
                MenuName = menuName,
                IsActive = false, // newly created menu is not active by default
                UpdatedAt = DateTime.UtcNow
            };

            var addedMenu = await _menuRepository.AddMenuAsync(menu);

            // Fetch all variants for the selected products and add to menu details
            var variantIds = new List<int>();
            foreach (var pid in productIds)
            {
                var variants = await _menuRepository.GetProductVariantsByProductIdAsync(pid);
                variantIds.AddRange(variants.Select(v => v.VariantId));
            }

            if (variantIds.Any())
            {
                await _menuRepository.UpdateMenuProductsAsync(addedMenu.MenuId, variantIds);
            }
        }

        public async Task UpdateMenuProductsAsync(int menuId, List<int> productIds)
        {
            var variantIds = new List<int>();
            foreach (var pid in productIds)
            {
                var variants = await _menuRepository.GetProductVariantsByProductIdAsync(pid);
                variantIds.AddRange(variants.Select(v => v.VariantId));
            }

            await _menuRepository.UpdateMenuProductsAsync(menuId, variantIds);

            var menu = await _menuRepository.GetMenuByIdAsync(menuId);
            if (menu != null)
            {
                menu.UpdatedAt = DateTime.UtcNow;
                await _menuRepository.UpdateMenuAsync(menu);
            }
        }
    }
}
