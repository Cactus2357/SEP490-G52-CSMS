using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Repositories.Interfaces;

namespace SEP490_G52_CSMS.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly CSMSAppDbContext _context;

        public ProductCategoryRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductCategory>> GetAllAsync()
        {
            return await _context.ProductCategories
                .Include(c => c.MasterProducts)
                    .ThenInclude(p => p.ProductVariants)
                .ToListAsync();
        }

        public async Task<ProductCategory?> GetByIdAsync(int id)
        {
            return await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeCategoryId = null)
        {
            return await _context.ProductCategories
                .AnyAsync(c => c.CategoryName == categoryName
                    && (!excludeCategoryId.HasValue || c.CategoryId != excludeCategoryId.Value));
        }

        public async Task AddAsync(ProductCategory productCategory)
        {
            _context.ProductCategories.Add(productCategory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductCategory productCategory)
        {
            _context.ProductCategories.Update(productCategory);
            await _context.SaveChangesAsync();
        }
    }
}