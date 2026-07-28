using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Repositories.Interfaces;

namespace SEP490_G52_CSMS.Repositories
{
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly CSMSAppDbContext _context;

        public ProductVariantRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId, string? searchString)
        {
            var query = _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(v => v.SizeVariant != null && v.SizeVariant.Contains(searchString));
            }

            return await query.ToListAsync();
        }

        public async Task<ProductVariant?> GetVariantByIdAsync(int variantId)
        {
            return await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.VariantId == variantId);
        }

        public async Task<bool> IsVariantNameExistsAsync(int productId, string sizeVariant, int? excludeVariantId = null)
        {
            var query = _context.ProductVariants
                .Where(v => v.ProductId == productId && v.SizeVariant == sizeVariant);

            if (excludeVariantId.HasValue)
            {
                query = query.Where(v => v.VariantId != excludeVariantId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task AddVariantAsync(ProductVariant variant)
        {
            await _context.ProductVariants.AddAsync(variant);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Update(variant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanDeleteVariantAsync(int variantId)
        {
            // Check if variant is used in OrderItems
            bool usedInOrders = await _context.OrderItems.AnyAsync(oi => oi.VariantId == variantId);
            
            // Check if variant is used in MenuDetails
            bool usedInMenus = await _context.MenuDetails.AnyAsync(md => md.VariantId == variantId);

            return !usedInOrders && !usedInMenus;
        }
    }
}
