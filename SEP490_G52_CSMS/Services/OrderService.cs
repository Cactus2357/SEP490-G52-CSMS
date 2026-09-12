using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
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

        public async Task<Order> CreateOrderAsync(
            string recipientName, 
            string branchId, 
            int cashierId, 
            List<OrderItem> items, 
            string? tableNumber = null, 
            string? customerName = null, 
            decimal? subtotalAmount = null, 
            decimal? discountAmount = null, 
            decimal? tradeDiscountAmount = null, 
            string? orderNotes = null,
            string? voucherCode = null)
        {
            var date = DateTime.Now;
            if (date.TimeOfDay < TimeSpan.FromHours(6))
            {
                throw new InvalidOperationException("Hệ thống chỉ mở bán hàng từ 06:00 đến 24:00 hàng ngày. Hiện tại đang ngoài khung giờ phục vụ.");
            }

            int countToday = await _orderRepo.GetOrdersCountByDateAsync(date);
            string orderId = $"MH{date:yyMMdd}-{(countToday + 1):D3}";

            decimal subtotal = subtotalAmount ?? items.Sum(i => i.Quantity * i.UnitPrice);
            decimal discount = discountAmount ?? 0;
            decimal tradeDiscount = tradeDiscountAmount ?? 0;

            int? appliedVoucherId = null;
            string? appliedVoucherCode = null;

            if (!string.IsNullOrWhiteSpace(voucherCode))
            {
                var cleanCode = voucherCode.Trim().ToUpper();
                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => 
                    v.VoucherCode.ToUpper() == cleanCode && 
                    (v.BranchId == branchId || v.BranchId == null) && 
                    v.IsActive);

                if (voucher != null && date >= voucher.StartDate && date <= voucher.EndDate && voucher.UsedCount < voucher.Quantity)
                {
                    appliedVoucherId = voucher.VoucherId;
                    appliedVoucherCode = voucher.VoucherCode;
                    
                    // Nếu client chưa tính discount hoặc gửi 0, tự động tính theo % của voucher
                    if (discount <= 0 && voucher.DiscountPercent > 0)
                    {
                        discount = Math.Round(subtotal * (voucher.DiscountPercent / 100m));
                        if (discount > subtotal) discount = subtotal;
                    }

                    // Tăng số lượt dùng của voucher
                    voucher.UsedCount += 1;
                }
            }

            decimal totalAmount = Math.Max(0, subtotal - discount - tradeDiscount);

            var finalCustomer = !string.IsNullOrWhiteSpace(customerName) ? customerName.Trim() : (string.IsNullOrWhiteSpace(recipientName) ? "Khách lẻ" : recipientName.Trim());
            var finalTable = !string.IsNullOrWhiteSpace(tableNumber) ? tableNumber.Trim() : "Mang về";
            var combinedRecipient = $"{finalTable} - {finalCustomer}";

            var order = new Order
            {
                OrderId = orderId,
                RecipientName = combinedRecipient,
                TableNumber = finalTable,
                CustomerName = finalCustomer,
                SubtotalAmount = subtotal,
                DiscountAmount = discount,
                TradeDiscountAmount = tradeDiscount,
                OrderNotes = orderNotes,
                VoucherId = appliedVoucherId,
                VoucherCode = appliedVoucherCode,
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

            decimal? tendered = null;
            decimal? change = null;
            string actualMethod = paymentMethod ?? "Cash";

            decimal remainingToPay = Math.Max(0, order.TotalAmount - order.PaidAmount);

            if (actualMethod == "Cash" && customerCash.HasValue && customerCash > 0)
            {
                tendered = customerCash.Value;
                change = changeAmount ?? Math.Max(0, customerCash.Value - order.TotalAmount);
                order.PaymentMethod = $"Cash (Đưa:{tendered.Value:N0}đ - Thừa:{change.Value:N0}đ)";
            }
            else if (order.CashPaid > 0 && actualMethod != "Cash")
            {
                order.PaymentMethod = $"Split (CK:{(order.BankPaid > 0 ? order.BankPaid : remainingToPay):N0}đ + TM:{order.CashPaid:N0}đ)";
            }
            else
            {
                order.PaymentMethod = actualMethod;
            }

            // Ghi nhận bản ghi thanh toán vào bảng payments nếu chưa thanh toán đủ
            if (remainingToPay > 0)
            {
                var payment = new Payment
                {
                    PaymentId = $"PAY-{order.OrderId}-{DateTime.UtcNow:HHmmss}-{Random.Shared.Next(100, 999)}",
                    OrderId = order.OrderId!,
                    BranchId = order.BranchId,
                    CashierId = order.CashierId,
                    PaymentType = "Payment",
                    PaymentMethod = actualMethod,
                    Amount = (actualMethod != "Cash" && order.CashPaid > 0) ? remainingToPay : (remainingToPay > 0 ? remainingToPay : order.TotalAmount),
                    Status = "Success",
                    TransactionCode = bankTransactionCode,
                    CustomerCash = tendered,
                    ChangeAmount = change,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);
            }

            order.PaymentStatus = "Paid";
            order.BrewingStatus = "Waiting for Brewing";

            await _context.SaveChangesAsync();

            // [BR-Inventory] Tự động trừ tồn kho theo Recipe ngay khi thanh toán thành công
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                await DeductInventoryForItemsAsync(order.BranchId, order.OrderItems.Select(oi => (oi.VariantId, oi.Quantity)));
            }

            return true;
        }

        public async Task<bool> ProcessSplitPaymentAsync(string orderId, decimal cashAmount, decimal bankAmount, decimal? customerCash = null, decimal? changeAmount = null, string? bankTransactionCode = null)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null || order.PaymentStatus == "Paid") return false;

            string cashDetail = "";
            decimal? tendered = null;
            decimal? change = null;
            if (customerCash.HasValue && customerCash > 0)
            {
                tendered = customerCash.Value;
                change = changeAmount ?? Math.Max(0, customerCash.Value - cashAmount);
                cashDetail = $" (Đưa:{tendered.Value:N0}đ - Thừa:{change.Value:N0}đ)";
            }

            order.PaymentMethod = $"Split (CK:{bankAmount:N0}đ + TM:{cashAmount:N0}đ{cashDetail})";

            // Ghi nhận 2 bản ghi thanh toán độc lập vào bảng payments:
            // 1. Chuyển khoản
            if (bankAmount > 0)
            {
                _context.Payments.Add(new Payment
                {
                    PaymentId = $"PAY-{order.OrderId}-BNK-{DateTime.UtcNow:HHmmss}",
                    OrderId = order.OrderId!,
                    BranchId = order.BranchId,
                    CashierId = order.CashierId,
                    PaymentType = "Payment",
                    PaymentMethod = "BankTransfer",
                    Amount = bankAmount,
                    Status = "Success",
                    TransactionCode = bankTransactionCode,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 2. Tiền mặt
            if (cashAmount > 0)
            {
                _context.Payments.Add(new Payment
                {
                    PaymentId = $"PAY-{order.OrderId}-CSH-{DateTime.UtcNow:HHmmss}",
                    OrderId = order.OrderId!,
                    BranchId = order.BranchId,
                    CashierId = order.CashierId,
                    PaymentType = "Payment",
                    PaymentMethod = "Cash",
                    Amount = cashAmount,
                    Status = "Success",
                    CustomerCash = tendered,
                    ChangeAmount = change,
                    CreatedAt = DateTime.UtcNow
                });
            }

            order.PaymentStatus = "Paid";
            order.BrewingStatus = "Waiting for Brewing";

            await _context.SaveChangesAsync();

            // [BR-Inventory] Tự động trừ tồn kho theo Recipe ngay khi thanh toán thành công
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                await DeductInventoryForItemsAsync(order.BranchId, order.OrderItems.Select(oi => (oi.VariantId, oi.Quantity)));
            }

            return true;
        }

        public async Task<(bool success, string message)> ProcessRefundCashAsync(string orderId, decimal refundAmount, string reason, int cashierId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null) return (false, "Không tìm thấy đơn hàng.");

            var now = order.CreatedAt.Kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;
            if ((now - order.CreatedAt).TotalHours > 24)
            {
                return (false, "Không thể hủy đơn hoặc hoàn tiền cho đơn hàng đã tạo quá 1 ngày (quá 24 giờ).");
            }

            decimal effectiveRefund = refundAmount > 0 ? refundAmount : order.TotalAmount;

            // Ghi nhận bản ghi hoàn tiền vào bảng payments
            _context.Payments.Add(new Payment
            {
                PaymentId = $"REF-{order.OrderId}-{DateTime.UtcNow:HHmmss}",
                OrderId = order.OrderId!,
                BranchId = order.BranchId,
                CashierId = cashierId,
                PaymentType = "Refund",
                PaymentMethod = "Cash",
                Amount = effectiveRefund,
                Status = "Success",
                Notes = reason,
                CreatedAt = DateTime.UtcNow
            });

            bool isPartial = (effectiveRefund < order.TotalAmount);
            if (isPartial)
            {
                order.PaymentStatus = "Partially Refunded";
                order.BrewingStatus = "Partially Refunded";
            }
            else
            {
                order.PaymentStatus = "Cancelled";
                order.BrewingStatus = "Cancelled / Refunded";

                // Hoàn trả nguyên liệu lại vào kho nếu hủy toàn bộ đơn hàng
                if (order.OrderItems != null && order.OrderItems.Any())
                {
                    await RestoreInventoryForItemsAsync(order.BranchId, order.OrderItems.Select(oi => (oi.VariantId, oi.Quantity)));
                }
            }

            await _context.SaveChangesAsync();

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

            return (true, "Đã hoàn tiền mặt thành công. Số tiền đã được trừ vào dòng tiền mặt của ca hiện tại.");
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
            if (order.OrderItems != null)
            {
                foreach (var oi in order.OrderItems)
                {
                    oi.IsCompleted = true;
                }
            }
            await _orderRepo.UpdateOrderAsync(order);
            return true;
        }

        public async Task<(bool Success, string Message, bool OrderCompleted, int CompletedItems, int TotalItems)> ToggleOrderItemBrewingAsync(string orderId, int variantId, bool? targetStatus = null)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return (false, "Đơn hàng không tồn tại.", false, 0, 0);
            }

            var item = order.OrderItems?.FirstOrDefault(oi => oi.VariantId == variantId);
            if (item == null)
            {
                return (false, "Món không tồn tại trong đơn hàng.", false, 0, 0);
            }

            bool newStatus = targetStatus.HasValue ? targetStatus.Value : !item.IsCompleted;
            item.IsCompleted = newStatus;

            int totalItems = order.OrderItems?.Count ?? 0;
            int completedItems = order.OrderItems?.Count(oi => oi.IsCompleted) ?? 0;
            bool allCompleted = totalItems > 0 && completedItems == totalItems;

            if (allCompleted)
            {
                if (order.BrewingStatus != "Completed" && order.BrewingStatus != "Done")
                {
                    order.BrewingStatus = "Completed";
                }
            }
            else
            {
                if (order.BrewingStatus == "Completed" || order.BrewingStatus == "Done")
                {
                    order.BrewingStatus = "Brewing in Progress";
                }
                else if (order.BrewingStatus == "Waiting for Brewing" && completedItems > 0)
                {
                    order.BrewingStatus = "Brewing in Progress";
                }
            }

            await _orderRepo.UpdateOrderAsync(order);

            string msg = allCompleted
                ? "Đơn hàng đã hoàn thành tất cả các món!"
                : (newStatus ? "Đã đánh dấu món hoàn thành." : "Đã chuyển món về đang pha chế.");

            return (true, msg, allCompleted, completedItems, totalItems);
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
            order.OrderNotes = $"[Báo thiếu NL: {missingNames}] Lý do: {reason ?? "Hết nguyên liệu pha chế"}. (Ước tính hoàn: {missingTotalAmount:N0}đ)";

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

            var now = order.CreatedAt.Kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;
            if ((now - order.CreatedAt).TotalHours > 24)
            {
                return (false, "Không thể sửa đơn hoặc đổi món/hoàn tiền cho đơn hàng đã tạo quá 1 ngày (quá 24 giờ).", 0, 0);
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
                string refundNotes = $"[Đổi món] Đơn gốc: {oldTotal:N0}đ, Đơn mới: {newTotal:N0}đ. Hoàn thừa: {refundDiff:N0}đ. Lý do: {reason ?? "Đổi món do thiếu nguyên liệu"}";

                var refundPayment = new Payment
                {
                    OrderId = order.OrderId,
                    BranchId = order.BranchId,
                    CashierId = cashierId,
                    PaymentType = "Refund",
                    PaymentMethod = "Cash",
                    Amount = refundDiff,
                    Status = "Success",
                    Notes = refundNotes,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Payments.AddAsync(refundPayment);
                order.Payments.Add(refundPayment);

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

                var addPayment = new Payment
                {
                    OrderId = order.OrderId,
                    BranchId = order.BranchId,
                    CashierId = cashierId,
                    PaymentType = "Payment",
                    PaymentMethod = paymentMethod == "Cash" ? "Cash" : "BankTransfer",
                    Amount = additionalAmount,
                    Status = "Success",
                    Notes = $"[Đổi món] Thu thêm: {additionalAmount:N0}đ ({addPayStr})",
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Payments.AddAsync(addPayment);
                order.Payments.Add(addPayment);

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
                else if (status == "Hoàn tiền 1 phần" || status == "Hoàn tiền một phần")
                {
                    orders = orders.Where(o => o.RefundAmount > 0 && o.RefundAmount < o.TotalAmount && o.BrewingStatus != "Missing Ingredients");
                }
                else if (status == "Đang chờ pha chế")
                {
                    orders = orders.Where(o => o.PaymentStatus == "Paid" && (o.BrewingStatus == "Waiting for Brewing" || o.BrewingStatus == "Waiting") && o.BrewingStatus != "Missing Ingredients");
                }
                else if (status == "Đang pha chế")
                {
                    orders = orders.Where(o => o.PaymentStatus == "Paid" && (o.BrewingStatus == "Brewing in Progress" || o.BrewingStatus == "Brewing") && o.BrewingStatus != "Missing Ingredients");
                }
                else if (status == "Chờ thanh toán")
                {
                    orders = orders.Where(o => o.PaymentStatus == "Unpaid" && o.BrewingStatus != "Cancelled / Refunded" && o.RefundAmount == 0);
                }
                else if (status == "Thanh toán 1 phần")
                {
                    orders = orders.Where(o => o.PaymentStatus == "PartiallyPaid" && o.BrewingStatus != "Cancelled / Refunded");
                }
                else if (status == "Đang xử lý")
                {
                    orders = orders.Where(o => o.BrewingStatus != "Completed" && o.BrewingStatus != "Done"
                                            && o.BrewingStatus != "Cancelled / Refunded" && o.BrewingStatus != "Missing Ingredients"
                                            && o.BrewingStatus != "Partially Refunded"
                                            && o.PaymentStatus != "Cancelled" && o.RefundAmount == 0);
                }
                else if (status == "Đã giao hàng" || status == "Đã hoàn thành" || status == "Hoàn thành")
                {
                    orders = orders.Where(o => o.BrewingStatus == "Delivered" && o.BrewingStatus != "Cancelled / Refunded" && o.PaymentStatus != "Cancelled" && o.RefundAmount == 0);
                }
                else if (status == "Đã pha chế xong")
                {
                    orders = orders.Where(o => (o.BrewingStatus == "Completed" || o.BrewingStatus == "Done") && o.BrewingStatus != "Delivered"
                                            && o.BrewingStatus != "Cancelled / Refunded" && o.PaymentStatus != "Cancelled" && o.RefundAmount == 0);
                }
                else if (status == "Đã hủy" || status == "Đã hủy / Hoàn tiền")
                {
                    orders = orders.Where(o => o.BrewingStatus == "Cancelled / Refunded" || o.PaymentStatus == "Cancelled" || o.RefundAmount >= o.TotalAmount);
                }
            }

            int pageSize = 15;
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
                PageSize = pageSize,
                Orders = pagedOrders.Select(o =>
                {
                    bool isMissing = (o.BrewingStatus == "Missing Ingredients");
                    bool isPartialRefund = (!isMissing && o.RefundAmount > 0 && o.RefundAmount < o.TotalAmount);
                    bool isCancelled = (!isMissing && (o.BrewingStatus == "Cancelled / Refunded" || o.PaymentStatus == "Cancelled" || o.RefundAmount >= o.TotalAmount));
                    bool isDelivered = !isCancelled && !isMissing && !isPartialRefund && (o.BrewingStatus == "Delivered");
                    bool isDone = !isCancelled && !isMissing && !isPartialRefund && !isDelivered && (o.BrewingStatus == "Completed" || o.BrewingStatus == "Done");
                    bool isUnpaid = !isCancelled && !isMissing && !isPartialRefund && !isDone && !isDelivered && (o.PaymentStatus == "Unpaid");
                    bool isPartiallyPaid = !isCancelled && !isMissing && !isPartialRefund && !isDone && !isDelivered && (o.PaymentStatus == "PartiallyPaid");
                    bool isBrewing = !isCancelled && !isMissing && !isPartialRefund && !isDone && !isDelivered && !isUnpaid && !isPartiallyPaid && (o.BrewingStatus == "Brewing in Progress" || o.BrewingStatus == "Brewing");

                    string displayStatus = isMissing ? "Thiếu nguyên liệu (chờ xử lý)" :
                                           (isPartialRefund ? "Hoàn tiền 1 phần" :
                                           (isCancelled ? "Đã hủy / Hoàn tiền" :
                                           (isDelivered ? "Đã giao hàng" :
                                           (isDone ? "Đã pha chế xong" :
                                           (isUnpaid ? "Chờ thanh toán" :
                                           (isPartiallyPaid ? "Thanh toán 1 phần" :
                                           (isBrewing ? "Đang pha chế" : "Đang chờ pha chế")))))));

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
                        CashAmount = o.CashAmount,
                        BankAmount = o.BankAmount,
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
                TableNumber = o.TableNumber,
                CustomerName = o.CustomerName,
                OrderNotes = o.OrderNotes,
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
                    UnitPrice = oi.UnitPrice,
                    IsCompleted = oi.IsCompleted
                }).ToList()
            });
        }

        public async Task<SaleOrderDetailViewModel?> GetOrderDetailsAsync(string orderId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null) return null;

            bool isMissing = (order.BrewingStatus == "Missing Ingredients");
            bool isPartialRefund = (!isMissing && order.RefundAmount > 0 && order.RefundAmount < order.TotalAmount);
            bool isCancelled = (!isMissing && (order.BrewingStatus == "Cancelled / Refunded" || order.PaymentStatus == "Cancelled" || order.RefundAmount >= order.TotalAmount));
            bool isDelivered = !isCancelled && !isMissing && !isPartialRefund && (order.BrewingStatus == "Delivered");
            bool isDone = !isCancelled && !isMissing && !isPartialRefund && !isDelivered && (order.BrewingStatus == "Completed" || order.BrewingStatus == "Done");
            bool isUnpaid = !isCancelled && !isMissing && !isPartialRefund && !isDone && !isDelivered && (order.PaymentStatus == "Unpaid");
            bool isPartiallyPaid = !isCancelled && !isMissing && !isPartialRefund && !isDone && !isDelivered && (order.PaymentStatus == "PartiallyPaid");
            bool isBrewing = !isCancelled && !isMissing && !isPartialRefund && !isDone && !isDelivered && !isUnpaid && !isPartiallyPaid && (order.BrewingStatus == "Brewing in Progress" || order.BrewingStatus == "Brewing");

            string displayStatus = isMissing ? "Thiếu nguyên liệu (chờ xử lý)" :
                                   (isPartialRefund ? "Hoàn tiền 1 phần" :
                                   (isCancelled ? "Đã hủy / Hoàn tiền" :
                                   (isDelivered ? "Đã giao hàng" :
                                   (isDone ? "Đã pha chế xong" :
                                   (isUnpaid ? "Chờ thanh toán" :
                                   (isPartiallyPaid ? "Thanh toán 1 phần" :
                                   (isBrewing ? "Đang pha chế" : "Đang chờ pha chế")))))));

            decimal missingAmount = 0;
            string? missingNote = order.OrderNotes ?? order.RefundReason;
            if (isMissing && !string.IsNullOrEmpty(missingNote) && missingNote.Contains("Ước tính hoàn:"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(missingNote, @"Ước tính hoàn:\s*([\d\.,]+)đ");
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
                TableNumber = !string.IsNullOrWhiteSpace(order.TableNumber) ? order.TableNumber : (order.RecipientName?.Contains(" - ") == true ? order.RecipientName.Split(" - ")[0] : "Mang về"),
                CustomerName = !string.IsNullOrWhiteSpace(order.CustomerName) ? order.CustomerName : (order.RecipientName?.Contains(" - ") == true ? order.RecipientName.Split(" - ")[1] : (order.RecipientName ?? "Khách lẻ")),
                SubtotalAmount = order.SubtotalAmount > 0 ? order.SubtotalAmount : order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                TradeDiscountAmount = order.TradeDiscountAmount,
                VoucherCode = order.VoucherCode,
                OrderNotes = order.OrderNotes,
                BranchName = order.Branch?.BranchName ?? "CSMS",
                BranchAddress = order.Branch?.Address ?? "",
                BranchPhone = order.Branch?.PhoneNumber ?? "",
                CashierName = order.Cashier?.FullName ?? $"Thu ngân #{order.CashierId}",
                OrderTime = order.CreatedAt,
                PaymentMethod = order.PaymentMethod ?? "",
                BankTransactionCode = order.BankTransactionCode,
                PaymentStatus = order.PaymentStatus,
                BrewingStatus = order.BrewingStatus,
                DisplayStatus = displayStatus,
                TotalAmount = order.TotalAmount,
                CashAmount = order.CashAmount,
                BankAmount = order.BankAmount,
                RefundAmount = order.RefundAmount,
                RefundReason = order.RefundReason,
                RefundMethod = order.RefundMethod,
                RefundedAt = order.RefundedAt,
                IsMissingIngredients = isMissing,
                MissingItemsAmount = missingAmount > 0 ? missingAmount : order.TotalAmount,
                MissingIngredientsDetail = missingNote,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    VariantId = oi.VariantId,
                    ProductName = oi.ProductVariant?.MasterProduct?.ProductName ?? "",
                    Size = oi.ProductVariant?.SizeVariant ?? "",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    IsCompleted = oi.IsCompleted
                }).ToList()
            };
        }

        public async Task<(bool success, string message)> MarkOrderAsDeliveredAsync(string orderId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return (false, "Không tìm thấy thông tin đơn hàng.");
            }

            if (order.PaymentStatus == "Cancelled" || order.BrewingStatus == "Cancelled / Refunded" || order.RefundAmount >= order.TotalAmount)
            {
                return (false, "Đơn hàng đã bị hủy hoặc hoàn tiền, không thể giao hàng.");
            }

            if (order.BrewingStatus == "Missing Ingredients")
            {
                return (false, "Đơn hàng đang bị thiếu nguyên liệu, vui lòng xử lý hoàn tiền hoặc sửa món trước khi giao.");
            }

            if (order.BrewingStatus == "Delivered")
            {
                return (true, "Đơn hàng đã ở trạng thái [Đã giao hàng] trước đó.");
            }

            if (order.BrewingStatus != "Completed" && order.BrewingStatus != "Done")
            {
                return (false, "Chỉ khi đơn hàng đã pha chế xong mới được cập nhật đã giao cho khách.");
            }

            order.BrewingStatus = "Delivered";
            await _orderRepo.UpdateOrderAsync(order);

            return (true, $"Đã cập nhật trạng thái đơn hàng {order.OrderId} thành: Đã giao hàng.");
        }
    }
}
