using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Repositories.Interfaces;

namespace SEP490_G52_CSMS.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CSMSAppDbContext _context;

        public ProductRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MasterProduct>> GetAllAsync()
        {
            return await _context.MasterProducts
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductVariants)
                .ToListAsync();
        }

        public async Task<MasterProduct?> GetByIdAsync(int id)
        {
            return await _context.MasterProducts
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<bool> IsProductNameExistsAsync(string productName, int? excludeProductId = null)
        {
            return await _context.MasterProducts
                .AnyAsync(p => p.ProductName == productName
                    && (!excludeProductId.HasValue || p.ProductId != excludeProductId.Value));
        }

        public async Task AddAsync(MasterProduct masterProduct)
        {
            _context.MasterProducts.Add(masterProduct);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MasterProduct masterProduct)
        {
            _context.MasterProducts.Update(masterProduct);
            await _context.SaveChangesAsync();
        }
    }
}