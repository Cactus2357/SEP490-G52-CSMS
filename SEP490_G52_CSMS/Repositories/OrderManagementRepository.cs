using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Repositories
{
    public class OrderManagementRepository : IOrderManagementRepository
    {
        private readonly CSMSAppDbContext _context;

        public OrderManagementRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetOrdersAsync(string branchId, string searchCashier, string status, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Orders
                .Include(o => o.Cashier)
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

            // Default business rule BR03: display current day if no date is specified
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                var today = DateTime.Today;
                var hasOrdersToday = await _context.Orders
                    .AnyAsync(o => (string.IsNullOrEmpty(branchId) || o.BranchId == branchId) && o.CreatedAt.Date == today);
                if (hasOrdersToday)
                {
                    query = query.Where(o => o.CreatedAt.Date == today);
                }
            }

            if (!string.IsNullOrEmpty(searchCashier))
            {
                // Find cashiers matching name
                query = query.Where(o => o.Cashier != null && (o.Cashier.FullName.Contains(searchCashier) || o.Cashier.Username.Contains(searchCashier)));
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                if (status == "Hoàn thành")
                {
                    query = query.Where(o => o.PaymentStatus == "Paid" && (o.BrewingStatus == "Done" || o.BrewingStatus == "Completed"));
                }
                else if (status == "Đang xử lý")
                {
                    query = query.Where(o => o.PaymentStatus == "Unpaid" || 
                                           o.BrewingStatus == "Waiting" || 
                                           o.BrewingStatus == "Brewing" || 
                                           o.BrewingStatus == "Waiting for Brewing" || 
                                           o.BrewingStatus == "Brewing in Progress");
                }
                else if (status == "Đã hủy")
                {
                    query = query.Where(o => o.PaymentStatus == "Canceled" || o.BrewingStatus == "Canceled");
                }
            }

            var list = await query.ToListAsync();
            return list.OrderBy(o => {
                bool isCanceled = o.PaymentStatus == "Canceled" || o.PaymentStatus == "Cancelled" || o.BrewingStatus == "Canceled" || o.BrewingStatus == "Cancelled";
                bool isCompleted = o.PaymentStatus == "Paid" && (o.BrewingStatus == "Done" || o.BrewingStatus == "Completed");
                
                if (!isCanceled && !isCompleted) return 0; // Đang xử lý
                if (isCompleted) return 1; // Hoàn thành
                return 2; // Đã hủy
            })
            .ThenBy(o => {
                bool isCanceled = o.PaymentStatus == "Canceled" || o.PaymentStatus == "Cancelled" || o.BrewingStatus == "Canceled" || o.BrewingStatus == "Cancelled";
                bool isCompleted = o.PaymentStatus == "Paid" && (o.BrewingStatus == "Done" || o.BrewingStatus == "Completed");
                
                if (!isCanceled && !isCompleted) return o.CreatedAt.Ticks; // Oldest first
                return -o.CreatedAt.Ticks; // Newest first
            })
            .ToList();
        }

        public async Task<Order?> GetOrderDetailsAsync(string orderId, string branchId)
        {
            var query = _context.Orders
                .Include(o => o.Cashier)
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
