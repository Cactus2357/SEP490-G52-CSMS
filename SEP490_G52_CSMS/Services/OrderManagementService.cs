using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class OrderManagementService : IOrderManagementService
    {
        private readonly IOrderManagementRepository _repository;

        public OrderManagementService(IOrderManagementRepository repository)
        {
            _repository = repository;
        }

        private string MapStatus(Order o)
        {
            bool isMissing = (o.BrewingStatus == "Missing Ingredients");
            bool isCancelled = (!isMissing && (o.BrewingStatus == "Cancelled / Refunded" || o.BrewingStatus == "Canceled" || o.BrewingStatus == "Cancelled" || o.PaymentStatus == "Cancelled" || o.PaymentStatus == "Canceled"));
            bool isDelivered = !isCancelled && !isMissing && (o.BrewingStatus == "Delivered");
            bool isDone = !isCancelled && !isMissing && !isDelivered && (o.BrewingStatus == "Completed" || o.BrewingStatus == "Done");
            bool isUnpaid = !isCancelled && !isMissing && !isDone && !isDelivered && (o.PaymentStatus == "Unpaid");
            bool isPartiallyPaid = !isCancelled && !isMissing && !isDone && !isDelivered && (o.PaymentStatus == "PartiallyPaid");
            bool isBrewing = !isCancelled && !isMissing && !isDone && !isDelivered && !isUnpaid && !isPartiallyPaid && (o.BrewingStatus == "Brewing in Progress" || o.BrewingStatus == "Brewing");

            if (isMissing) return "Thiếu nguyên liệu";
            if (isCancelled) return "Đã hủy";
            if (isDelivered) return "Đã giao hàng";
            if (isDone) return "Đã pha chế xong";
            if (isUnpaid) return "Chờ thanh toán";
            if (isPartiallyPaid) return "Thanh toán 1 phần";
            if (isBrewing) return "Đang pha chế";
            return "Đang chờ pha chế";
        }

        public async Task<OrderManagementListViewModel> GetOrderManagementListAsync(
            string branchId,
            string branchName,
            string searchCashier,
            string status,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? cursor = null,
            string direction = "next",
            int pageSize = 15)
        {
            DateTime? cursorDate = null;
            if (!string.IsNullOrEmpty(cursor) && long.TryParse(cursor, out long ticks))
            {
                cursorDate = new DateTime(ticks);
            }

            var data = await _repository.GetOrdersAsync(branchId, searchCashier, status, fromDate, toDate, cursorDate, direction, pageSize);

            bool hasNext = false;
            bool hasPrev = false;

            if (direction == "next")
            {
                if (data.Count > pageSize)
                {
                    hasNext = true;
                    data.RemoveAt(data.Count - 1);
                }
                if (cursorDate.HasValue)
                {
                    hasPrev = true;
                }
            }
            else // direction == "prev"
            {
                if (data.Count > pageSize)
                {
                    hasPrev = true;
                    data.RemoveAt(0);
                }
                hasNext = true;
            }

            var model = new OrderManagementListViewModel
            {
                BranchId = branchId,
                BranchName = branchName,
                SearchCashier = searchCashier,
                FilterStatus = status,
                FromDate = fromDate,
                ToDate = toDate,
                HasNext = hasNext,
                HasPrev = hasPrev
            };

            foreach (var item in data)
            {
                model.Items.Add(new OrderManagementItemViewModel
                {
                    OrderId = item.OrderId ?? "",
                    CashierName = item.Cashier?.FullName ?? item.Cashier?.Username ?? "Unknown",
                    TotalItems = item.OrderItems.Sum(oi => oi.Quantity),
                    TotalAmount = item.TotalAmount,
                    Status = MapStatus(item),
                    CreatedAt = item.CreatedAt
                });
            }

            if (model.Items.Any())
            {
                model.NextCursor = model.Items.Last().CreatedAt.Ticks.ToString();
                model.PrevCursor = model.Items.First().CreatedAt.Ticks.ToString();
            }

            return model;
        }

        public async Task<OrderDetailViewModel?> GetOrderDetailAsync(string orderId, string branchId)
        {
            var order = await _repository.GetOrderDetailsAsync(orderId, branchId);
            if (order == null) return null;

            var model = new OrderDetailViewModel
            {
                OrderId = order.OrderId ?? "",
                Status = MapStatus(order),
                CashierName = order.Cashier?.FullName ?? order.Cashier?.Username ?? "Unknown",
                ShiftInfo = "Ca sáng - CN Q1", // Faked for now as per design
                PaymentMethod = order.PaymentMethod ?? "Cash",
                TransactionId = "#TXN" + new Random().Next(1000, 9999), // Mock transaction id
                TotalItems = order.OrderItems.Sum(oi => oi.Quantity),
                TotalAmount = order.TotalAmount,
                CreatedAtStr = order.CreatedAt.EnsureVietnamTimeString("HH:mm"),
                PaidAtStr = order.CreatedAt.AddMinutes(2).EnsureVietnamTimeString("HH:mm"),
                CompletedAtStr = order.CreatedAt.AddMinutes(4).EnsureVietnamTimeString("HH:mm")
            };

            foreach (var item in order.OrderItems)
            {
                string pName = item.ProductVariant?.MasterProduct?.ProductName ?? "Sản phẩm";
                string size = item.ProductVariant?.SizeVariant ?? "M";

                model.Items.Add(new OrderDetailItemViewModel
                {
                    ProductName = pName,
                    Size = size,
                    Quantity = item.Quantity,
                    TotalPrice = item.Quantity * item.UnitPrice
                });
            }

            return model;
        }
    }
}
