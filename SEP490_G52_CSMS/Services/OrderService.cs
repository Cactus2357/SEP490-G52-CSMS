using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Reponsitories;
using Microsoft.EntityFrameworkCore;

namespace SEP490_G52_CSMS.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly CSMSAppDbContext _context;

        public OrderService(IOrderRepository orderRepo, CSMSAppDbContext context)
        {
            _orderRepo = orderRepo;
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(string recipientName, string branchId, int cashierId, List<OrderItem> items)
        {
            var date = DateTime.Now;
            int countToday = await _orderRepo.GetOrdersCountByDateAsync(date);
            string orderId = $"MH{date:yyMMdd}-{(countToday + 1):D3}";

            decimal totalAmount = items.Sum(i => i.Quantity * i.UnitPrice);

            var order = new Order
            {
                OrderId = orderId,
                RecipientName = recipientName,
                BranchId = branchId,
                CashierId = cashierId,
                CreatedAt = date,
                TotalAmount = totalAmount,
                PaymentStatus = "Unpaid",
                BrewingStatus = "Waiting",
                OrderItems = items
            };

            return await _orderRepo.CreateOrderAsync(order);
        }

        public async Task<bool> ProcessPaymentAsync(string orderId, string paymentMethod)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null || order.PaymentStatus == "Paid") return false;

            order.PaymentMethod = paymentMethod;
            order.PaymentStatus = "Paid";
            // Once paid, it is ready for brewing
            order.BrewingStatus = "Waiting for Brewing"; 
            
            await _orderRepo.UpdateOrderAsync(order);
            return true;
        }

        public async Task<bool> StartBrewingAsync(string orderId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null || order.BrewingStatus != "Waiting for Brewing") return false;

            order.BrewingStatus = "Brewing in Progress";
            await _orderRepo.UpdateOrderAsync(order);
            return true;
        }

        public async Task<bool> CompleteBrewingAsync(string orderId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null || order.BrewingStatus != "Brewing in Progress") return false;

            order.BrewingStatus = "Completed";
            await _orderRepo.UpdateOrderAsync(order);

            // BR05: Auto-Deduction on Order Completion
            try
            {
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .Include(oi => oi.ProductVariant)
                    .ToListAsync();

                foreach (var oi in orderItems)
                {
                    if (oi.ProductVariant != null)
                    {
                        var recipes = await _context.Recipes
                            .Where(r => r.VariantId == oi.VariantId)
                            .Include(r => r.Material)
                            .ToListAsync();

                        foreach (var r in recipes)
                        {
                            if (r.Material != null)
                            {
                                var branchInv = await _context.BranchInventories
                                    .FirstOrDefaultAsync(bi => bi.BranchId == order.BranchId && bi.MaterialId == r.MaterialId);

                                if (branchInv != null)
                                {
                                    // Conversion: Recipe quantity in g/ml, storage unit in kg/lít. Convert by dividing by 1000.
                                    decimal deduction = (r.Quantity * oi.Quantity) / 1000m;
                                    branchInv.StockQuantity -= deduction;
                                    if (branchInv.StockQuantity < 0)
                                    {
                                        branchInv.StockQuantity = 0; // Clamp at 0 to prevent negative stock
                                    }
                                }
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Silently log or ignore to prevent blocking order completions
            }

            return true;
        }

        public async Task<OrderHistoryViewModel> GetOrderHistoryAsync(string branchId, string status, DateTime? fromDate, DateTime? toDate, string search, int page = 1)
        {
            var orders = await _orderRepo.GetAllOrdersAsync();

            if (!string.IsNullOrEmpty(branchId))
            {
                orders = orders.Where(o => o.BranchId == branchId);
            }

            if (fromDate.HasValue)
                orders = orders.Where(o => o.CreatedAt.Date >= fromDate.Value.Date);
            if (toDate.HasValue)
                orders = orders.Where(o => o.CreatedAt.Date <= toDate.Value.Date);
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                orders = orders.Where(o => o.OrderId.ToLower().Contains(search) || 
                                           (o.RecipientName != null && o.RecipientName.ToLower().Contains(search)));
            }
            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                if (status == "Đang xử lý")
                    orders = orders.Where(o => o.BrewingStatus != "Completed" && o.BrewingStatus != "Done");
                else if (status == "Đã hoàn thành")
                    orders = orders.Where(o => o.BrewingStatus == "Completed" || o.BrewingStatus == "Done");
            }

            int pageSize = 10;
            int totalOrders = orders.Count();
            int totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = new OrderHistoryViewModel
            {
                StatusFilter = status ?? "Tất cả",
                FromDate = fromDate,
                ToDate = toDate,
                SearchKeyword = search ?? "",
                CurrentPage = page,
                TotalPages = totalPages,
                Orders = pagedOrders.Select(o => new OrderSummaryViewModel
                {
                    OrderId = o.OrderId,
                    RecipientName = o.RecipientName ?? "",
                    OrderTime = o.CreatedAt,
                    PaymentStatus = o.PaymentStatus,
                    BrewingStatus = o.BrewingStatus,
                    DisplayStatus = (o.BrewingStatus == "Completed" || o.BrewingStatus == "Done") ? "đã hoàn thành" : "đang xử lý"
                }).ToList()
            };

            return vm;
        }

        public async Task<IEnumerable<OrderSummaryViewModel>> GetWaitingAndBrewingOrdersAsync()
        {
            var waiting = await _orderRepo.GetOrdersByStatusAsync("Paid", "Waiting for Brewing");
            var brewing = await _orderRepo.GetOrdersByStatusAsync("Paid", "Brewing in Progress");

            var combined = waiting.Concat(brewing).OrderBy(o => o.CreatedAt).ToList();

            return combined.Select(o => new OrderSummaryViewModel
            {
                OrderId = o.OrderId,
                RecipientName = string.IsNullOrWhiteSpace(o.RecipientName) ? "Khách lẻ" : o.RecipientName,
                OrderTime = o.CreatedAt,
                PaymentStatus = o.PaymentStatus,
                BrewingStatus = o.BrewingStatus,
                DisplayStatus = o.BrewingStatus == "Waiting for Brewing" ? "đang chờ pha chế" : "đang trong quá trình pha chế",
                Items = o.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.ProductVariant?.MasterProduct?.ProductName ?? "",
                    Size = oi.ProductVariant?.SizeVariant ?? "",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            });
        }

        public async Task<SaleOrderDetailViewModel?> GetOrderDetailsAsync(string orderId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null) return null;

            return new SaleOrderDetailViewModel
            {
                OrderId = order.OrderId,
                RecipientName = order.RecipientName ?? "",
                OrderTime = order.CreatedAt,
                DisplayStatus = order.BrewingStatus == "Completed" ? "đã hoàn thành" : 
                                (order.BrewingStatus == "Waiting for Brewing" ? "đang chờ pha chế" : "đang trong quá trình pha chế"),
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.ProductVariant?.MasterProduct?.ProductName ?? "",
                    Size = oi.ProductVariant?.SizeVariant ?? "",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }
    }
}
