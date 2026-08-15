using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Repositories.Interfaces;

namespace SEP490_G52_CSMS.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly CSMSAppDbContext _context;

        public OrderRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> GetOrderByIdAsync(string orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.MasterProduct)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.MasterProduct)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetOrdersCountByDateAsync(DateTime date)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt.Date == date.Date)
                .CountAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string paymentStatus, string brewingStatus, string? branchId = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.MasterProduct)
                .AsQueryable();

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(o => o.BranchId == branchId);
            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                query = query.Where(o => o.PaymentStatus == paymentStatus);
            }
            if (!string.IsNullOrEmpty(brewingStatus))
            {
                query = query.Where(o => o.BrewingStatus == brewingStatus);
            }

            return await query.OrderBy(o => o.CreatedAt).ToListAsync();
        }
    }
}
