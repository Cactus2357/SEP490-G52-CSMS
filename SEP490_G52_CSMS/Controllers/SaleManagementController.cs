using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services;
using SEP490_G52_CSMS.Reponsitories;
using SEP490_G52_CSMS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    public class SaleManagementController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IMenuRepository _menuRepo;
        private readonly CSMSAppDbContext _context;
        
        public SaleManagementController(IOrderService orderService, IMenuRepository menuRepo, CSMSAppDbContext context)
        {
            _orderService = orderService;
            _menuRepo = menuRepo;
            _context = context;
        }

        private async Task<string> GetUserBranchIdAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == userId);
                if (employee != null && !string.IsNullOrEmpty(employee.BranchId))
                {
                    return employee.BranchId;
                }
            }
            return "CB004"; // Default fallback
        }

        private async Task<int> GetUserCashierIdAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            return 5; // Default fallback (cashier is 5)
        }

        public async Task<IActionResult> CreateOrder()
        {
            var branchId = await GetUserBranchIdAsync();
            var menus = await _menuRepo.GetMenusByBranchAsync(branchId);
            var activeMenu = menus?.FirstOrDefault(m => m.IsActive);
            BranchMenu? menu = null;
            if (activeMenu != null)
            {
                menu = await _menuRepo.GetMenuByIdAsync(activeMenu.MenuId);
            }
            
            var vm = new CreateOrderViewModel();
            
            var allCategories = await _menuRepo.GetProductCategoriesAsync();
            var allMasterProducts = await _menuRepo.GetMasterProductsAsync();

            var categories = new List<SaleCategoryViewModel>();
            foreach (var cat in allCategories)
            {
                categories.Add(new SaleCategoryViewModel
                {
                    CategoryId = cat.CategoryId,
                    CategoryName = cat.CategoryName ?? "Other"
                });
            }

            var menuProducts = new List<SaleProductViewModel>();
            foreach (var master in allMasterProducts)
            {
                if (master.ProductVariants == null || !master.ProductVariants.Any()) continue;

                var pvm = new SaleProductViewModel
                {
                    ProductId = master.ProductId,
                    ProductName = master.ProductName ?? "",
                    CategoryId = master.CategoryId,
                    CategoryName = master.ProductCategory?.CategoryName ?? "Other",
                    ImageUrl = master.ImageUrl ?? "",
                    Variants = master.ProductVariants.Select(pv => new SaleVariantViewModel
                    {
                        VariantId = pv.VariantId,
                        SizeVariant = pv.SizeVariant ?? "S",
                        SellingPrice = pv.SellingPrice
                    }).ToList()
                };

                var def = pvm.Variants.FirstOrDefault(v => v.SizeVariant == "S") ?? pvm.Variants.FirstOrDefault();
                if (def != null)
                {
                    pvm.DefaultVariantId = def.VariantId;
                    pvm.DefaultPrice = def.SellingPrice;
                }
                menuProducts.Add(pvm);
            }

            vm.Categories = categories;
            vm.MenuProducts = menuProducts;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder([FromBody] OrderSubmissionModel model)
        {
            if (model == null || !model.Items.Any())
            {
                return BadRequest(new { success = false, message = "Invalid order data" });
            }

            var recipient = string.IsNullOrWhiteSpace(model.RecipientName) ? "Khách lẻ" : model.RecipientName.Trim();

            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();

            var orderItems = model.Items.Select(i => new OrderItem
            {
                VariantId = i.VariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList();

            var order = await _orderService.CreateOrderAsync(recipient, branchId, cashierId, orderItems);
            
            return Json(new { success = true, orderId = order.OrderId });
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string orderId, string paymentMethod)
        {
            bool result = await _orderService.ProcessPaymentAsync(orderId, paymentMethod);
            if (result) return Json(new { success = true });
            return BadRequest(new { success = false, message = "Payment failed" });
        }

        [HttpGet]
        public async Task<IActionResult> BankTransferPayment(string orderId)
        {
            var details = await _orderService.GetOrderDetailsAsync(orderId);
            if (details == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("CreateOrder");
            }
            return View(details);
        }

        [HttpGet]
        public IActionResult CustomerMonitor()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SelectBankTransfer(string orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound(new { success = false, message = "Order not found" });

            order.PaymentMethod = "Bank Transfer";
            order.PaymentStatus = "Unpaid";
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> SimulateBankTransferSuccess(string orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound(new { success = false, message = "Order not found" });

            order.PaymentStatus = "TransferSuccessPending";
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(string orderId)
        {
            var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound(new { success = false, message = "Order not found" });

            return Json(new { success = true, paymentStatus = order.PaymentStatus });
        }

        [HttpPost]
        public async Task<IActionResult> CancelBankTransfer(string orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order != null && order.PaymentStatus == "Unpaid")
            {
                order.PaymentStatus = "Cancelled";
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        public async Task<IActionResult> OrderHistory(string status, DateTime? fromDate, DateTime? toDate, string search, int page = 1)
        {
            var branchId = await GetUserBranchIdAsync();
            var vm = await _orderService.GetOrderHistoryAsync(branchId, status, fromDate, toDate, search, page);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(string orderId)
        {
            var details = await _orderService.GetOrderDetailsAsync(orderId);
            if (details == null) return NotFound();
            
            return PartialView("_OrderDetailPartial", details);
        }
    }

    public class OrderSubmissionModel
    {
        public string RecipientName { get; set; } = "";
        public List<OrderItemSubmission> Items { get; set; } = new List<OrderItemSubmission>();
    }

    public class OrderItemSubmission
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
