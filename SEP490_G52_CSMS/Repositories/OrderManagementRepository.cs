using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Repositories.Interfaces;

namespace SEP490_G52_CSMS.Repositories
{
    public class OrderManagementRepository : IOrderManagementRepository
    {
        private readonly CSMSAppDbContext _context;

        public OrderManagementRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetOrdersAsync(
            string branchId,
            string searchCashier,
            string status,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            DateTime? cursor = null,
            string direction = "next",
            int pageSize = 10)
        {
            var query = _context.Orders
                .Include(o => o.Cashier)
                .Include(o => o.Payments)
                .Include(o => o.OrderItems)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(o => o.BranchId == branchId);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt.Date >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt.Date <= toDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(searchCashier))
            {
                // Find cashiers matching name
                query = query.Where(o => o.Cashier != null && (o.Cashier.FullName.Contains(searchCashier) || o.Cashier.Username.Contains(searchCashier)));
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                if (status == "Đã giao hàng")
                {
                    query = query.Where(o => o.BrewingStatus == "Delivered");
                }
                else if (status == "Hoàn thành")
                {
                    query = query.Where(o => o.PaymentStatus == "Paid" || o.BrewingStatus == "Done" || o.BrewingStatus == "Completed" || o.BrewingStatus == "Delivered");
                }
                else if (status == "Đã hủy")
                {
                    query = query.Where(o => o.PaymentStatus == "Canceled" || o.PaymentStatus == "Cancelled" || o.BrewingStatus == "Canceled" || o.BrewingStatus == "Cancelled");
                }
                else if (status == "Đang xử lý")
                {
                    query = query.Where(o => o.PaymentStatus != "Paid" &&
                                           o.PaymentStatus != "Canceled" &&
                                           o.PaymentStatus != "Cancelled" &&
                                           o.BrewingStatus != "Done" &&
                                           o.BrewingStatus != "Completed" &&
                                           o.BrewingStatus != "Delivered" &&
                                           o.BrewingStatus != "Canceled" &&
                                           o.BrewingStatus != "Cancelled");
                }
            }

            // Cursor pagination logic
            if (cursor.HasValue)
            {
                if (direction == "prev")
                {
                    query = query.Where(o => o.CreatedAt > cursor.Value);
                    query = query.OrderBy(o => o.CreatedAt); // Ascending order
                }
                else
                {
                    query = query.Where(o => o.CreatedAt < cursor.Value);
                    query = query.OrderByDescending(o => o.CreatedAt); // Descending order
                }
            }
            else
            {
                query = query.OrderByDescending(o => o.CreatedAt); // Descending order
            }

            var list = await query.Take(pageSize + 1).ToListAsync();

            if (direction == "prev")
            {
                list.Reverse();
            }

            return list;
        }

        public async Task<Order?> GetOrderDetailsAsync(string orderId, string branchId)
        {
            var query = _context.Orders
                .Include(o => o.Cashier)
                .Include(o => o.Payments)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.MasterProduct)
                .AsNoTracking()
                .Where(o => o.OrderId == orderId);

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(o => o.BranchId == branchId);
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}
