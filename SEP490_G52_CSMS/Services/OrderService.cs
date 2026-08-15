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
        private readonly INotificationService _notificationService;

        public OrderService(IOrderRepository orderRepo, CSMSAppDbContext context, INotificationService notificationService)
        {
            _orderRepo = orderRepo;
            _context = context;
            _notificationService = notificationService;
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
            order.BrewingStatus = "Waiting for Brewing";

            await _orderRepo.UpdateOrderAsync(order);

            // [BR-Inventory] Tự động trừ tồn kho theo Recipe ngay khi thanh toán thành công
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                await DeductInventoryForItemsAsync(order.BranchId, order.OrderItems.Select(oi => (oi.VariantId, oi.Quantity)));
            }

            return true;
        }

        public async Task<bool> ProcessRefundCashAsync(string orderId, decimal refundAmount, string reason, int cashierId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null) return false;

            decimal effectiveRefund = refundAmount > 0 ? refundAmount : order.TotalAmount;
            order.RefundAmount = effectiveRefund;
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
                    activeHandover.CashRefundAmount += effectiveRefund;
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
            return true;
        }

        public async Task<bool> ReportMissingIngredientsAsync(string orderId, List<int> missingVariantIds, string? reason, int bartenderUserId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv!.MasterProduct)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null || order.OrderItems == null || !order.OrderItems.Any())
            {
                return false;
            }

            var missingItems = order.OrderItems
                .Where(oi => missingVariantIds.Contains(oi.VariantId))
                .ToList();

            if (!missingItems.Any())
            {
                missingItems = order.OrderItems.ToList();
            }

            decimal missingTotalAmount = missingItems.Sum(oi => oi.Quantity * oi.UnitPrice);
            string missingNames = string.Join(", ", missingItems.Select(oi => $"{oi.ProductVariant?.MasterProduct?.ProductName} ({oi.ProductVariant?.SizeVariant} x{oi.Quantity})"));

            order.BrewingStatus = "Missing Ingredients";
            order.RefundReason = $"[Báo thiếu NL: {missingNames}] Lý do: {reason ?? "Hết nguyên liệu pha chế"}. (Ước tính hoàn: {missingTotalAmount:N0}đ)";

            // Hoàn trả lại nguyên liệu của các món bị thiếu vào kho chi nhánh
            await RestoreInventoryForItemsAsync(order.BranchId, missingItems.Select(oi => (oi.VariantId, oi.Quantity)));

            await _context.SaveChangesAsync();

            // Gửi thông báo real-time tới đúng Cashier đã tạo đơn hàng
            try
            {
                await _notificationService.SendAsync(new NotificationEvent(
                    Title: $"⚠️ Đơn {order.OrderId} - Thiếu nguyên liệu pha chế!",
                    Message: $"Bartender báo thiếu món: {missingNames}. Số tiền cần xử lý/hoàn: {missingTotalAmount:N0} đ. Lý do: {reason ?? "Hết nguyên liệu"}",
                    RecipientUserId: order.CashierId,
                    RecipientRole: "Cashier",
                    ResourceUrl: $"/SaleManagement/OrderHistory?search={order.OrderId}",
                    BranchId: order.BranchId
                ));
            }
            catch (Exception)
            {
                // Silently continue if notification fails
            }

            return true;
        }

        public async Task<(bool success, string message, decimal refundDifference, decimal additionalAmount)> ExchangeOrderItemsAsync(
            string orderId, List<OrderItemExchangeSubmission> newItems, string paymentMethod, decimal? customerCash, decimal? changeAmount, string? reason, int cashierId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv!.MasterProduct)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return (false, "Không tìm thấy đơn hàng cần sửa.", 0, 0);
            }

            if (newItems == null || !newItems.Any())
            {
                return (false, "Danh sách món mới không được để trống.", 0, 0);
            }

            decimal oldTotal = order.TotalAmount;
            decimal newTotal = newItems.Sum(i => i.Quantity * i.UnitPrice);
            decimal diff = newTotal - oldTotal;

            decimal refundDiff = 0;
            decimal additionalAmount = 0;

            var today = DateTime.Today;
            var activeHandover = await _context.CashHandovers
                .FirstOrDefaultAsync(ch => ch.BranchId == order.BranchId && ch.HandoverDate.Date == today && ch.Status == "Active");

            if (diff < 0)
            {
                // Tổng mới < Tổng cũ -> Hoàn lại phần tiền thừa cho khách
                refundDiff = Math.Abs(diff);
                order.RefundAmount += refundDiff;
                order.RefundReason = $"[Đổi món] Đơn gốc: {oldTotal:N0}đ, Đơn mới: {newTotal:N0}đ. Hoàn thừa: {refundDiff:N0}đ. Lý do: {reason ?? "Đổi món do thiếu nguyên liệu"}";
                order.RefundMethod = "Cash";
                order.RefundedAt = DateTime.Now;

                if (activeHandover != null)
                {
                    activeHandover.CashRefundAmount += refundDiff;
                    activeHandover.TheoreticalCash = activeHandover.InitialCash + activeHandover.MachineCashRevenue - activeHandover.CashRefundAmount;
                }
            }
            else if (diff > 0)
            {
                // Tổng mới > Tổng cũ -> Thu thêm tiền từ khách
                additionalAmount = diff;
                string addPayStr = paymentMethod == "Cash" ? "Tiền mặt" : "Chuyển khoản";
                order.PaymentMethod = $"{order.PaymentMethod} + Thu thêm {addPayStr} ({additionalAmount:N0}đ)";

                if (activeHandover != null)
                {
                    if (paymentMethod == "Cash")
                    {
                        activeHandover.MachineCashRevenue += additionalAmount;
                        activeHandover.TheoreticalCash = activeHandover.InitialCash + activeHandover.MachineCashRevenue - activeHandover.CashRefundAmount;
                    }
                    else
                    {
                        activeHandover.BankTransferRevenue += additionalAmount;
                    }
                }
            }

            // Xóa các OrderItem cũ và cập nhật OrderItem mới
            _context.OrderItems.RemoveRange(order.OrderItems);

            var createdOrderItems = newItems.Select(ni => new OrderItem
            {
                OrderId = order.OrderId!,
                VariantId = ni.VariantId,
                Quantity = ni.Quantity,
                UnitPrice = ni.UnitPrice
            }).ToList();

            await _context.OrderItems.AddRangeAsync(createdOrderItems);

            // Trừ kho nguyên liệu cho các món trong đơn hàng mới
            await DeductInventoryForItemsAsync(order.BranchId, createdOrderItems.Select(oi => (oi.VariantId, oi.Quantity)));

            order.TotalAmount = newTotal;
            order.BrewingStatus = "Waiting for Brewing"; // Đẩy lại hàng đợi cho Bartender

            await _context.SaveChangesAsync();

            // Gửi thông báo cho Bartender về việc đơn đã được đổi món
            try
            {
                await _notificationService.SendAsync(new NotificationEvent(
                    Title: $"✅ Đơn {order.OrderId} đã đổi món thành công",
                    Message: $"Thu ngân đã cập nhật lại danh sách món cho đơn {order.OrderId}. Đơn đã sẵn sàng để pha chế lại.",
                    RecipientRole: "Bartender",
                    ResourceUrl: "/Brewing/Index",
                    BranchId: order.BranchId
                ));
            }
            catch (Exception)
            {
                // Silently ignore
            }

            return (true, "Đã cập nhật đổi món thành công và đẩy lại hàng đợi pha chế!", refundDiff, additionalAmount);
        }

        private async Task DeductInventoryForItemsAsync(string? branchId, IEnumerable<(int VariantId, int Quantity)> items)
        {
            if (string.IsNullOrEmpty(branchId)) return;

            try
            {
                foreach (var (variantId, quantity) in items)
                {
                    var recipes = await _context.Recipes
                        .Where(r => r.VariantId == variantId && (r.BranchId == null || r.BranchId == branchId))
                        .ToListAsync();

                    foreach (var r in recipes)
                    {
                        var branchInv = await _context.BranchInventories
                            .FirstOrDefaultAsync(bi => bi.BranchId == branchId && bi.MaterialId == r.MaterialId);

                        if (branchInv != null)
                        {
                            decimal deduction = (r.Quantity * quantity) / 1000m;
                            branchInv.StockQuantity -= deduction;
                            if (branchInv.StockQuantity < 0)
                            {
                                branchInv.StockQuantity = 0;
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Prevent blocking sales if stock table has minor discrepancy
            }
        }

        private async Task RestoreInventoryForItemsAsync(string? branchId, IEnumerable<(int VariantId, int Quantity)> items)
        {
            if (string.IsNullOrEmpty(branchId)) return;

            try
            {
                foreach (var (variantId, quantity) in items)
                {
                    var recipes = await _context.Recipes
                        .Where(r => r.VariantId == variantId && (r.BranchId == null || r.BranchId == branchId))
                        .ToListAsync();

                    foreach (var r in recipes)
                    {
                        var branchInv = await _context.BranchInventories
                            .FirstOrDefaultAsync(bi => bi.BranchId == branchId && bi.MaterialId == r.MaterialId);

                        if (branchInv != null)
                        {
                            decimal restoration = (r.Quantity * quantity) / 1000m;
                            branchInv.StockQuantity += restoration;
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Ignore restoration error
            }
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
                if (status == "Thiếu nguyên liệu / Chờ xử lý" || status == "Thiếu nguyên liệu")
                {
                    orders = orders.Where(o => o.BrewingStatus == "Missing Ingredients");
                }
                else if (status == "Đang xử lý")
                {
                    orders = orders.Where(o => o.BrewingStatus != "Completed" && o.BrewingStatus != "Done"
                                            && o.BrewingStatus != "Cancelled / Refunded" && o.BrewingStatus != "Missing Ingredients"
                                            && o.PaymentStatus != "Cancelled" && o.RefundAmount == 0);
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
                Orders = pagedOrders.Select(o =>
                {
                    bool isMissing = (o.BrewingStatus == "Missing Ingredients");
                    bool isCancelled = (o.BrewingStatus == "Cancelled / Refunded" || o.PaymentStatus == "Cancelled" || o.RefundAmount > 0);
                    bool isDone = !isCancelled && !isMissing && (o.BrewingStatus == "Completed" || o.BrewingStatus == "Done");

                    string displayStatus = isMissing ? "thiếu nguyên liệu (chờ xử lý)" :
                                           (isCancelled ? "Đã hủy / Hoàn tiền" :
                                           (isDone ? "đã hoàn thành" : "đang xử lý"));

                    return new OrderSummaryViewModel
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
                        DisplayStatus = displayStatus,
                        IsMissingIngredients = isMissing,
                        Items = o.OrderItems.Select(oi => new OrderItemViewModel
                        {
                            VariantId = oi.VariantId,
                            ProductName = oi.ProductVariant?.MasterProduct?.ProductName ?? "",
                            Size = oi.ProductVariant?.SizeVariant ?? "",
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice
                        }).ToList()
                    };
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
                    VariantId = oi.VariantId,
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

            bool isMissing = (order.BrewingStatus == "Missing Ingredients");
            bool isCancelled = (order.BrewingStatus == "Cancelled / Refunded" || order.PaymentStatus == "Cancelled" || order.RefundAmount > 0);
            bool isDone = !isCancelled && !isMissing && (order.BrewingStatus == "Completed" || order.BrewingStatus == "Done");

            string displayStatus = isMissing ? "thiếu nguyên liệu (chờ xử lý)" :
                                   (isCancelled ? "Đã hủy / Hoàn tiền" :
                                   (isDone ? "đã hoàn thành" :
                                   (order.BrewingStatus == "Waiting for Brewing" ? "đang chờ pha chế" : "đang trong quá trình pha chế")));

            decimal missingAmount = 0;
            if (isMissing && !string.IsNullOrEmpty(order.RefundReason) && order.RefundReason.Contains("Ước tính hoàn:"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(order.RefundReason, @"Ước tính hoàn:\s*([\d\.,]+)đ");
                if (match.Success)
                {
                    string numStr = match.Groups[1].Value.Replace(".", "").Replace(",", "");
                    decimal.TryParse(numStr, out missingAmount);
                }
            }

            return new SaleOrderDetailViewModel
            {
                OrderId = order.OrderId,
                RecipientName = order.RecipientName ?? "",
                OrderTime = order.CreatedAt,
                PaymentMethod = order.PaymentMethod ?? "",
                BankTransactionCode = order.BankTransactionCode,
                PaymentStatus = order.PaymentStatus,
                DisplayStatus = displayStatus,
                TotalAmount = order.TotalAmount,
                RefundAmount = order.RefundAmount,
                RefundReason = order.RefundReason,
                RefundMethod = order.RefundMethod,
                RefundedAt = order.RefundedAt,
                IsMissingIngredients = isMissing,
                MissingItemsAmount = missingAmount > 0 ? missingAmount : order.TotalAmount,
                MissingIngredientsDetail = order.RefundReason,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    VariantId = oi.VariantId,
                    ProductName = oi.ProductVariant?.MasterProduct?.ProductName ?? "",
                    Size = oi.ProductVariant?.SizeVariant ?? "",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }
    }
}
