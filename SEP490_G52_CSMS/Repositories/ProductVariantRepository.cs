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
                var allVariants = await query.ToListAsync();
                return allVariants.Where(v => v.SizeVariant != null && SEP490_G52_CSMS.Commons.StringHelper.FuzzyMatch(v.SizeVariant, searchString)).ToList();
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

        public async Task<List<Material>> SearchMaterialsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return await _context.Materials.Take(10).ToListAsync();
            }

            if (_context.Database.IsSqlServer())
            {
                var collateResults = await _context.Materials
                    .Where(m => EF.Functions.Collate(m.MaterialName, "SQL_Latin1_General_CP1_CI_AI").Contains(term))
                    .Take(20)
                    .ToListAsync();
                if (collateResults.Any())
                {
                    return collateResults;
                }
            }

            var allMaterials = await _context.Materials.Take(200).ToListAsync();
            return allMaterials
                .Where(m => SEP490_G52_CSMS.Commons.StringHelper.FuzzyMatch(m.MaterialName, term))
                .Take(20)
                .ToList();
        }

        public async Task<List<Recipe>> GetRecipeAsync(int variantId)
        {
            return await _context.Recipes
                .Include(r => r.Material)
                .Where(r => r.VariantId == variantId && r.BranchId == null)
                .ToListAsync();
        }

        public async Task SaveRecipeAsync(int variantId, List<Recipe> recipeItems)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _context.Recipes
                    .Where(r => r.VariantId == variantId && r.BranchId == null)
                    .ToListAsync();
                _context.Recipes.RemoveRange(existing);

                foreach (var item in recipeItems)
                {
                    item.VariantId = variantId;
                    item.BranchId = null; // RManager saves global recipe
                    await _context.Recipes.AddAsync(item);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
