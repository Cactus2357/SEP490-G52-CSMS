using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize]
    public class SaleManagementController : Controller
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string OrderId, DateTime CreatedAt)> _idempotencyStore = new();
        private readonly IOrderService _orderService;
        private readonly IMenuRepository _menuRepo;
        private readonly CSMSAppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public SaleManagementController(IOrderService orderService, IMenuRepository menuRepo, CSMSAppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _orderService = orderService;
            _menuRepo = menuRepo;
            _context = context;
            _httpClientFactory = httpClientFactory;
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

        private async Task<bool> IsEmployeeInActiveShiftAsync(int employeeId, string branchId)
        {
            if (User.IsInRole("RManager") || User.IsInRole("BranchManager"))
            {
                return true;
            }

            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var nowTime = DateTime.Now.TimeOfDay;

            // Query rosters assigned for today OR assigned for yesterday (if overnight shift spilled into early morning today)
            var rosterShifts = await _context.WeeklyRosterGrids
                .Include(r => r.FixedShift)
                .Where(r => r.EmployeeId == employeeId && (r.AssignmentDate.Date == today || r.AssignmentDate.Date == yesterday))
                .ToListAsync();

            bool inScheduledShift = rosterShifts.Any(r => {
                if (r.FixedShift == null) return false;
                var start = r.FixedShift.StartTime;
                var end = r.FixedShift.EndTime;

                if (start <= end)
                {
                    // Normal daytime shift
                    return r.AssignmentDate.Date == today && nowTime >= start && nowTime <= end;
                }
                else
                {
                    // Overnight shift (e.g., 22:00 - 06:00)
                    if (r.AssignmentDate.Date == today)
                    {
                        // Assigned today, current time is in evening (>= 22:00)
                        return nowTime >= start;
                    }
                    else if (r.AssignmentDate.Date == yesterday)
                    {
                        // Assigned yesterday, current time is early morning today (<= 06:00)
                        return nowTime <= end;
                    }
                    return false;
                }
            });

            bool hasOpenShift = await _context.CashHandovers
                .AnyAsync(ch => ch.BranchId == branchId 
                             && (ch.OutgoingCashierId == employeeId || ch.IncomingCashierId == employeeId) 
                             && (ch.HandoverDate.Date == today || ch.HandoverDate.Date == yesterday)
                             && (ch.ClosedAt == null || ch.Status == "Active"));

            return inScheduledShift || hasOpenShift;
        }

        public async Task<IActionResult> CreateOrder()
        {
            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();
            bool isInShift = await IsEmployeeInActiveShiftAsync(cashierId, branchId);
            if (!isInShift)
            {
                ViewBag.NotInShift = true;
                ViewBag.NotInShiftMessage = "Bạn hiện không ở trong ca làm việc (hoặc chưa mở ca). Vui lòng mở ca hoặc kiểm tra phân công lịch làm việc để thực hiện bán hàng.";
            }

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
        public async Task<IActionResult> SubmitAndPayCash([FromBody] CashPaymentSubmissionModel model)
        {
            if (model == null || model.Items == null || !model.Items.Any())
            {
                return BadRequest(new { success = false, message = "Dữ liệu đơn hàng không hợp lệ." });
            }

            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();

            if (!await IsEmployeeInActiveShiftAsync(cashierId, branchId))
            {
                return BadRequest(new { success = false, message = "Bạn hiện không ở trong ca làm việc nên không thể thực hiện thanh toán." });
            }

            var recipient = string.IsNullOrWhiteSpace(model.RecipientName) ? "Khách lẻ" : model.RecipientName.Trim();

            var orderItems = model.Items.Select(i => new OrderItem
            {
                VariantId = i.VariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList();

            var order = await _orderService.CreateOrderAsync(recipient, branchId, cashierId, orderItems);
            if (order == null || string.IsNullOrEmpty(order.OrderId))
            {
                return BadRequest(new { success = false, message = "Không thể khởi tạo đơn hàng." });
            }

            bool paid = await _orderService.ProcessPaymentAsync(order.OrderId, "Cash", model.CustomerCash, model.ChangeAmount);
            if (!paid)
            {
                return BadRequest(new { success = false, message = "Lỗi khi xử lý thanh toán tiền mặt." });
            }

            return Json(new { success = true, orderId = order.OrderId });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrderForTransfer([FromBody] OrderSubmissionModel model)
        {
            if (model == null || model.Items == null || !model.Items.Any())
            {
                return BadRequest(new { success = false, message = "Dữ liệu đơn hàng không hợp lệ." });
            }

            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();

            if (!await IsEmployeeInActiveShiftAsync(cashierId, branchId))
            {
                return BadRequest(new { success = false, message = "Bạn hiện không ở trong ca làm việc nên không thể bán hàng." });
            }

            var recipient = string.IsNullOrWhiteSpace(model.RecipientName) ? "Khách lẻ" : model.RecipientName.Trim();

            var orderItems = model.Items.Select(i => new OrderItem
            {
                VariantId = i.VariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList();

            var order = await _orderService.CreateOrderAsync(recipient, branchId, cashierId, orderItems);
            if (order == null || string.IsNullOrEmpty(order.OrderId))
            {
                return BadRequest(new { success = false, message = "Không thể khởi tạo đơn hàng." });
            }

            order.PaymentMethod = "Bank Transfer";
            order.PaymentStatus = "Unpaid";
            await _context.SaveChangesAsync();

            return Json(new { success = true, orderId = order.OrderId });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder([FromBody] OrderSubmissionModel model)
        {
            return await SubmitOrderForTransfer(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string orderId, string paymentMethod, decimal? customerCash = null, decimal? changeAmount = null)
        {
            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();
            if (!await IsEmployeeInActiveShiftAsync(cashierId, branchId))
            {
                return BadRequest(new { success = false, message = "Bạn hiện không ở trong ca làm việc nên không thể thực hiện thanh toán." });
            }

            bool result = await _orderService.ProcessPaymentAsync(orderId, paymentMethod, customerCash, changeAmount);
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

            DbInitializer.EnsureTablesCreated(_context);
            var branchId = await GetUserBranchIdAsync();
            var branchSetting = await _context.BranchSettings.FirstOrDefaultAsync(s => s.BranchId == branchId && s.IsSePayActive);
            if (branchSetting == null)
            {
                branchSetting = new Models.Core.BranchSetting
                {
                    BankCode = "MBBank",
                    AccountNumber = "0333333333",
                    AccountName = "CSMS CAFE",
                    TransferPrefix = "CSMS"
                };
            }

            ViewBag.BranchSetting = branchSetting;
            return View(details);
        }

        [HttpGet]
        [AllowAnonymous]
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
            order.PaymentMethod = $"Bank Transfer (SePay Sim - Đã nhận:{order.TotalAmount:N0}đ)";
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(string orderId)
        {
            DbInitializer.EnsureTablesCreated(_context);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound(new { success = false, message = "Order not found" });

            decimal receivedAmount = order.TotalAmount;
            if (!string.IsNullOrEmpty(order.PaymentMethod) && order.PaymentMethod.Contains("Đã nhận:"))
            {
                var match = Regex.Match(order.PaymentMethod, @"Đã nhận:([\d\.,]+)đ?");
                if (match.Success)
                {
                    string numStr = match.Groups[1].Value.Replace(".", "").Replace(",", "");
                    if (decimal.TryParse(numStr, out decimal parsed))
                    {
                        receivedAmount = parsed;
                    }
                }
            }

            return Json(new { 
                success = true, 
                paymentStatus = order.PaymentStatus,
                receivedAmount = receivedAmount
            });
        }

        [HttpPost]
        public async Task<IActionResult> CancelBankTransfer(string orderId)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order != null && (order.PaymentStatus == "Unpaid" || order.PaymentStatus == "Cancelled"))
                {
                    _context.OrderItems.RemoveRange(order.OrderItems);
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                }
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
        public string? IdempotencyKey { get; set; }
        public List<OrderItemSubmission> Items { get; set; } = new List<OrderItemSubmission>();
    }

    public class CashPaymentSubmissionModel : OrderSubmissionModel
    {
        public decimal CustomerCash { get; set; }
        public decimal ChangeAmount { get; set; }
    }

    public class OrderItemSubmission
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
