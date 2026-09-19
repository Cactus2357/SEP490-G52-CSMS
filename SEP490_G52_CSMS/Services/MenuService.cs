using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly CSMSAppDbContext _context;
        private readonly INotificationService _notificationService;

        public MenuService(IMenuRepository menuRepository, CSMSAppDbContext context, INotificationService notificationService)
        {
            _menuRepository = menuRepository;
            _context = context;
            _notificationService = notificationService;
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
                BranchId = menu.BranchId ?? string.Empty,
                IsActive = menu.IsActive
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

        public async Task<BartenderProductAvailabilityViewModel> GetBranchProductAvailabilityAsync(string branchId, string? search = null, int? categoryId = null, string? status = null)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchId == branchId);
            var branchName = branch?.BranchName ?? branchId;

            var activeMenu = await _menuRepository.GetActiveMenuWithDetailsAsync(branchId);
            if (activeMenu == null)
            {
                var menus = await _menuRepository.GetMenusByBranchAsync(branchId);
                var firstMenu = menus.FirstOrDefault();
                if (firstMenu != null)
                {
                    await _menuRepository.SetActiveMenuAsync(branchId, firstMenu.MenuId);
                    activeMenu = await _menuRepository.GetActiveMenuWithDetailsAsync(branchId);
                }
                else
                {
                    var allActiveMaster = await _menuRepository.GetMasterProductsAsync();
                    if (allActiveMaster.Any())
                    {
                        await AddMenuAsync(branchId, $"Thực đơn {branchId}", allActiveMaster.Select(p => p.ProductId).ToList());
                        var newMenus = await _menuRepository.GetMenusByBranchAsync(branchId);
                        if (newMenus.Any())
                        {
                            await _menuRepository.SetActiveMenuAsync(branchId, newMenus.First().MenuId);
                            activeMenu = await _menuRepository.GetActiveMenuWithDetailsAsync(branchId);
                        }
                    }
                }
            }

            var allMasterProducts = await _menuRepository.GetMasterProductsAsync();
            var allCategories = await _menuRepository.GetProductCategoriesAsync();

            var allItems = new List<BartenderProductItemViewModel>();

            foreach (var p in allMasterProducts)
            {
                var menuDetailsForProduct = activeMenu?.MenuDetails?
                    .Where(md => md.ProductVariant != null && md.ProductVariant.ProductId == p.ProductId)
                    .ToList() ?? new List<MenuDetail>();

                bool isAvailable = true;
                DateTime? updatedAt = null;
                string? updatedByName = null;

                if (menuDetailsForProduct.Any())
                {
                    isAvailable = menuDetailsForProduct.All(md => md.IsAvailable);

                    var latestUpdate = menuDetailsForProduct
                        .Where(md => md.UpdatedAt.HasValue)
                        .OrderByDescending(md => md.UpdatedAt)
                        .FirstOrDefault();

                    if (latestUpdate != null)
                    {
                        updatedAt = latestUpdate.UpdatedAt;
                        updatedByName = latestUpdate.Updater?.FullName ?? latestUpdate.Updater?.Username;
                    }
                }

                var sizesAndPrices = string.Join(" ", p.ProductVariants.Select(v => $"{v.SizeVariant}-{v.SellingPrice / 1000}K"));

                var variantsVm = p.ProductVariants.Select(v =>
                {
                    var md = menuDetailsForProduct.FirstOrDefault(d => d.VariantId == v.VariantId);
                    return new SaleVariantViewModel
                    {
                        VariantId = v.VariantId,
                        SizeVariant = v.SizeVariant ?? "S",
                        SellingPrice = v.SellingPrice,
                        IsAvailable = md == null || md.IsAvailable
                    };
                }).ToList();

                allItems.Add(new BartenderProductItemViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName ?? string.Empty,
                    CategoryId = p.CategoryId,
                    CategoryName = p.ProductCategory?.CategoryName ?? string.Empty,
                    ImageUrl = p.ImageUrl,
                    IsAvailable = isAvailable,
                    UpdatedAt = updatedAt,
                    UpdatedByName = updatedByName,
                    SizeAndPrice = sizesAndPrices,
                    Variants = variantsVm
                });
            }

            int totalCount = allItems.Count;
            int availableCount = allItems.Count(i => i.IsAvailable);
            int outOfStockCount = allItems.Count(i => !i.IsAvailable);

            var filtered = allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(i => SEP490_G52_CSMS.Commons.StringHelper.FuzzyMatch(i.ProductName, search));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                filtered = filtered.Where(i => i.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var st = status.Trim().ToLower();
                if (st == "available")
                {
                    filtered = filtered.Where(i => i.IsAvailable);
                }
                else if (st == "outofstock")
                {
                    filtered = filtered.Where(i => !i.IsAvailable);
                }
            }

            var orderedList = filtered
                .OrderBy(i => i.IsAvailable)
                .ThenBy(i => i.CategoryName)
                .ThenBy(i => i.ProductName)
                .ToList();

            return new BartenderProductAvailabilityViewModel
            {
                BranchId = branchId,
                BranchName = branchName,
                TotalProducts = totalCount,
                AvailableProducts = availableCount,
                OutOfStockProducts = outOfStockCount,
                SearchQuery = search,
                SelectedCategoryId = categoryId,
                StatusFilter = string.IsNullOrWhiteSpace(status) ? "all" : status.ToLower(),
                Categories = allCategories.Select(c => new SaleCategoryViewModel { CategoryId = c.CategoryId, CategoryName = c.CategoryName ?? "" }).ToList(),
                Products = orderedList
            };
        }

        public async Task<(bool success, string message)> SetProductAvailabilityAsync(string branchId, int productId, bool isAvailable, int updatedByEmployeeId)
        {
            var activeMenu = await _menuRepository.GetActiveMenuWithDetailsAsync(branchId);
            if (activeMenu == null)
            {
                var menus = await _menuRepository.GetMenusByBranchAsync(branchId);
                var firstMenu = menus.FirstOrDefault();
                if (firstMenu != null)
                {
                    await _menuRepository.SetActiveMenuAsync(branchId, firstMenu.MenuId);
                    activeMenu = await _menuRepository.GetActiveMenuWithDetailsAsync(branchId);
                }
                else
                {
                    var allActiveMaster = await _menuRepository.GetMasterProductsAsync();
                    if (allActiveMaster.Any())
                    {
                        await AddMenuAsync(branchId, $"Thực đơn {branchId}", allActiveMaster.Select(p => p.ProductId).ToList());
                        var newMenus = await _menuRepository.GetMenusByBranchAsync(branchId);
                        if (newMenus.Any())
                        {
                            await _menuRepository.SetActiveMenuAsync(branchId, newMenus.First().MenuId);
                            activeMenu = await _menuRepository.GetActiveMenuWithDetailsAsync(branchId);
                        }
                    }
                }
            }

            if (activeMenu == null)
            {
                return (false, "Không tìm thấy thực đơn hoạt động của chi nhánh.");
            }

            bool updated = await _menuRepository.UpdateMenuDetailsAvailabilityAsync(activeMenu.MenuId, productId, isAvailable, updatedByEmployeeId);
            if (!updated)
            {
                return (false, "Không thể cập nhật trạng thái món.");
            }

            var product = await _context.MasterProducts.FirstOrDefaultAsync(p => p.ProductId == productId);
            string prodName = product?.ProductName ?? $"Món #{productId}";

            try
            {
                var updater = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == updatedByEmployeeId);
                string updaterName = updater?.FullName ?? updater?.Username ?? "Bartender";

                await _notificationService.SendAsync(new NotificationEvent(
                    Title: isAvailable ? $"✅ Món [{prodName}] đã phục vụ trở lại" : $"⚠️ Món [{prodName}] tạm ngưng phục vụ (Hết NL)",
                    Message: isAvailable
                        ? $"{updaterName} đã mở lại món [{prodName}]."
                        : $"{updaterName} đã đánh dấu tạm hết món [{prodName}] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.",
                    RecipientRole: "Cashier",
                    ResourceUrl: "/SaleManagement/CreateOrder",
                    BranchId: branchId
                ));
            }
            catch
            {
                // Ignore notification failure
            }

            string msg = isAvailable ? $"Đã mở lại phục vụ món [{prodName}]." : $"Đã đánh dấu tạm hết món [{prodName}].";
            return (true, msg);
        }

        public async Task<(bool success, string message, bool newState)> ToggleProductAvailabilityAsync(string branchId, int productId, int updatedByEmployeeId)
        {
            var activeMenu = await _menuRepository.GetActiveMenuWithDetailsAsync(branchId);
            var menuDetailsForProduct = activeMenu?.MenuDetails?
                .Where(md => md.ProductVariant != null && md.ProductVariant.ProductId == productId)
                .ToList() ?? new List<MenuDetail>();

            bool currentlyAvailable = true;
            if (menuDetailsForProduct.Any())
            {
                currentlyAvailable = menuDetailsForProduct.All(md => md.IsAvailable);
            }

            bool newState = !currentlyAvailable;
            var (success, msg) = await SetProductAvailabilityAsync(branchId, productId, newState, updatedByEmployeeId);
            return (success, msg, newState);
        }
    }
}
