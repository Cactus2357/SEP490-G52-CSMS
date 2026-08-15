using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

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

        public async Task<bool> ProcessPaymentAsync(string orderId, string paymentMethod, decimal? customerCash = null, decimal? changeAmount = null, string? bankTransactionCode = null)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null || order.PaymentStatus == "Paid") return false;

            if (paymentMethod == "Cash" && customerCash.HasValue && customerCash > 0)
            {
                decimal change = changeAmount ?? Math.Max(0, customerCash.Value - order.TotalAmount);
                order.PaymentMethod = $"Cash (Đưa:{customerCash.Value:N0}đ - Thừa:{change:N0}đ)";
            }
            else
            {
                order.PaymentMethod = paymentMethod;
            }

            if (!string.IsNullOrEmpty(bankTransactionCode))
            {
                order.BankTransactionCode = bankTransactionCode;
            }

            order.PaymentStatus = "Paid";
            // Once paid, it is ready for brewing
            order.BrewingStatus = "Waiting for Brewing";

            await _orderRepo.UpdateOrderAsync(order);
            return true;
        }

        public async Task<bool> ProcessRefundCashAsync(string orderId, decimal refundAmount, string reason, int cashierId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null) return false;

            order.RefundAmount = refundAmount > 0 ? refundAmount : order.TotalAmount;
            order.RefundReason = reason;
            order.RefundMethod = "Cash";
            order.RefundedAt = DateTime.Now;
            order.BrewingStatus = "Cancelled / Refunded";

            await _orderRepo.UpdateOrderAsync(order);

            // Cập nhật dòng tiền mặt của ca hiện tại nếu có ca đang mở
            if (!string.IsNullOrEmpty(order.BranchId))
            {
                var today = DateTime.Today;
                var activeHandover = await _context.CashHandovers
                    .FirstOrDefaultAsync(ch => ch.BranchId == order.BranchId && ch.HandoverDate.Date == today && ch.Status == "Active");

                if (activeHandover != null)
                {
                    activeHandover.CashRefundAmount += order.RefundAmount;
                    activeHandover.TheoreticalCash = activeHandover.InitialCash + activeHandover.MachineCashRevenue - activeHandover.CashRefundAmount;
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> StartBrewingAsync(string orderId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null) return false;

            order.BrewingStatus = "Brewing in Progress";
            await _orderRepo.UpdateOrderAsync(order);
            return true;
        }

        public async Task<bool> CompleteBrewingAsync(string orderId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null) return false;

            if (order.BrewingStatus == "Completed" || order.BrewingStatus == "Done")
            {
                return true;
            }

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
                {
                    orders = orders.Where(o => o.BrewingStatus != "Completed" && o.BrewingStatus != "Done"
                                            && o.BrewingStatus != "Cancelled / Refunded" && o.PaymentStatus != "Cancelled" && o.RefundAmount == 0);
                }
                else if (status == "Đã hoàn thành")
                {
                    orders = orders.Where(o => (o.BrewingStatus == "Completed" || o.BrewingStatus == "Done")
                                            && o.BrewingStatus != "Cancelled / Refunded" && o.PaymentStatus != "Cancelled" && o.RefundAmount == 0);
                }
                else if (status == "Đã hủy" || status == "Đã hủy / Hoàn tiền")
                {
                    orders = orders.Where(o => o.BrewingStatus == "Cancelled / Refunded" || o.PaymentStatus == "Cancelled" || o.RefundAmount > 0);
                }
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
                    PaymentMethod = o.PaymentMethod ?? "",
                    BankTransactionCode = o.BankTransactionCode,
                    PaymentStatus = o.PaymentStatus,
                    BrewingStatus = o.BrewingStatus,
                    TotalAmount = o.TotalAmount,
                    RefundAmount = o.RefundAmount,
                    RefundReason = o.RefundReason,
                    DisplayStatus = (o.BrewingStatus == "Cancelled / Refunded" || o.PaymentStatus == "Cancelled" || o.RefundAmount > 0)
                                    ? "Đã hủy / Hoàn tiền"
                                    : ((o.BrewingStatus == "Completed" || o.BrewingStatus == "Done") ? "đã hoàn thành" : "đang xử lý")
                }).ToList()
            };

            return vm;
        }

        public async Task<IEnumerable<OrderSummaryViewModel>> GetWaitingAndBrewingOrdersAsync(string? branchId = null)
        {
            var waiting = await _orderRepo.GetOrdersByStatusAsync("Paid", "Waiting for Brewing", branchId);
            var brewing = await _orderRepo.GetOrdersByStatusAsync("Paid", "Brewing in Progress", branchId);

            var combined = waiting.Concat(brewing).OrderBy(o => o.CreatedAt).ToList();

            return combined.Select(o => new OrderSummaryViewModel
            {
                OrderId = o.OrderId,
                RecipientName = string.IsNullOrWhiteSpace(o.RecipientName) ? "Khách lẻ" : o.RecipientName,
                OrderTime = o.CreatedAt,
                PaymentMethod = o.PaymentMethod ?? "",
                BankTransactionCode = o.BankTransactionCode,
                PaymentStatus = o.PaymentStatus,
                BrewingStatus = o.BrewingStatus,
                TotalAmount = o.TotalAmount,
                RefundAmount = o.RefundAmount,
                RefundReason = o.RefundReason,
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
                PaymentMethod = order.PaymentMethod ?? "",
                BankTransactionCode = order.BankTransactionCode,
                PaymentStatus = order.PaymentStatus,
                DisplayStatus = (order.BrewingStatus == "Cancelled / Refunded" || order.PaymentStatus == "Cancelled" || order.RefundAmount > 0)
                                ? "Đã hủy / Hoàn tiền"
                                : (order.BrewingStatus == "Completed" ? "đã hoàn thành" :
                                  (order.BrewingStatus == "Waiting for Brewing" ? "đang chờ pha chế" : "đang trong quá trình pha chế")),
                TotalAmount = order.TotalAmount,
                RefundAmount = order.RefundAmount,
                RefundReason = order.RefundReason,
                RefundMethod = order.RefundMethod,
                RefundedAt = order.RefundedAt,
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
