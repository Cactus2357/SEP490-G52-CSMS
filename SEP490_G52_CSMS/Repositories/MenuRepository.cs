using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Repositories.Interfaces;

namespace SEP490_G52_CSMS.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly CSMSAppDbContext _context;

        public MenuRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BranchMenu>> GetMenusByBranchAsync(string branchId)
        {
            return await _context.BranchMenus
                .Where(m => m.BranchId == branchId)
                .OrderByDescending(m => m.UpdatedAt)
                .ToListAsync();
        }

        public async Task<BranchMenu?> GetMenuByIdAsync(int menuId)
        {
            return await _context.BranchMenus
                .Include(m => m.MenuDetails)
                    .ThenInclude(md => md.ProductVariant)
                        .ThenInclude(pv => pv.MasterProduct)
                            .ThenInclude(mp => mp.ProductCategory)
                .FirstOrDefaultAsync(m => m.MenuId == menuId);
        }

        public async Task<BranchMenu> AddMenuAsync(BranchMenu menu)
        {
            _context.BranchMenus.Add(menu);
            await _context.SaveChangesAsync();
            return menu;
        }

        public async Task UpdateMenuAsync(BranchMenu menu)
        {
            _context.BranchMenus.Update(menu);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMenuProductsAsync(int menuId, List<int> variantIds)
        {
            var existingDetails = await _context.MenuDetails.Where(md => md.MenuId == menuId).ToListAsync();
            _context.MenuDetails.RemoveRange(existingDetails);

            var newDetails = variantIds.Select(vid => new MenuDetail
            {
                MenuId = menuId,
                VariantId = vid
            }).ToList();

            _context.MenuDetails.AddRange(newDetails);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MasterProduct>> GetMasterProductsAsync(int? categoryId = null)
        {
            var query = _context.MasterProducts
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductVariants)
                .Where(p => p.Status == "Active");

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<ProductCategory>> GetProductCategoriesAsync()
        {
            return await _context.ProductCategories.ToListAsync();
        }

        public async Task SetActiveMenuAsync(string branchId, int activeMenuId)
        {
            var branchMenus = await _context.BranchMenus.Where(m => m.BranchId == branchId).ToListAsync();
            foreach (var menu in branchMenus)
            {
                menu.IsActive = menu.MenuId == activeMenuId;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductVariant>> GetProductVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(pv => pv.ProductId == productId)
                .ToListAsync();
        }

        public async Task<BranchMenu?> GetActiveMenuWithDetailsAsync(string branchId)
        {
            return await _context.BranchMenus
                .Include(m => m.MenuDetails)
                    .ThenInclude(md => md.ProductVariant)
                        .ThenInclude(pv => pv.MasterProduct)
                            .ThenInclude(mp => mp.ProductCategory)
                .Include(m => m.MenuDetails)
                    .ThenInclude(md => md.Updater)
                .FirstOrDefaultAsync(m => m.BranchId == branchId && m.IsActive);
        }

        public async Task<bool> UpdateMenuDetailsAvailabilityAsync(int menuId, int productId, bool isAvailable, int updatedByEmployeeId)
        {
            var details = await _context.MenuDetails
                .Include(md => md.ProductVariant)
                .Where(md => md.MenuId == menuId && md.ProductVariant != null && md.ProductVariant.ProductId == productId)
                .ToListAsync();

            var now = DateTime.UtcNow;

            if (!details.Any())
            {
                var variants = await _context.ProductVariants.Where(pv => pv.ProductId == productId).ToListAsync();
                if (!variants.Any()) return false;

                foreach (var v in variants)
                {
                    _context.MenuDetails.Add(new MenuDetail
                    {
                        MenuId = menuId,
                        VariantId = v.VariantId,
                        IsAvailable = isAvailable,
                        UpdatedBy = updatedByEmployeeId,
                        UpdatedAt = now
                    });
                }
            }
            else
            {
                foreach (var d in details)
                {
                    d.IsAvailable = isAvailable;
                    d.UpdatedBy = updatedByEmployeeId;
                    d.UpdatedAt = now;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
