using System;
using System.Linq;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories;

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
            if (o.PaymentStatus == "Paid" && o.BrewingStatus == "Done")
                return "Hoàn thành";
            if (o.PaymentStatus == "Canceled" || o.BrewingStatus == "Canceled")
                return "Đã hủy";
            return "Đang xử lý";
        }

        public async Task<OrderManagementListViewModel> GetOrderManagementListAsync(string branchId, string branchName, string searchCashier, string status)
        {
            var data = await _repository.GetOrdersAsync(branchId, searchCashier, status);

            var model = new OrderManagementListViewModel
            {
                BranchId = branchId,
                BranchName = branchName,
                SearchCashier = searchCashier,
                FilterStatus = status
            };

            foreach (var item in data)
            {
                model.Items.Add(new OrderManagementItemViewModel
                {
                    OrderId = item.OrderId ?? "",
                    CashierName = item.Cashier?.FullName ?? item.Cashier?.Username ?? "Unknown",
                    TotalItems = item.OrderItems.Sum(oi => oi.Quantity),
                    TotalAmount = item.TotalAmount,
                    Status = MapStatus(item)
                });
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
                CreatedAtStr = order.CreatedAt.ToString("HH:mm"),
                PaidAtStr = order.CreatedAt.AddMinutes(2).ToString("HH:mm"),
                CompletedAtStr = order.CreatedAt.AddMinutes(4).ToString("HH:mm")
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
