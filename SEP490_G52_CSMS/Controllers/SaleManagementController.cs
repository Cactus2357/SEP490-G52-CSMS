using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
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
        private readonly ICashHandoverService _cashHandoverService;

        public SaleManagementController(
            IOrderService orderService,
            IMenuRepository menuRepo,
            CSMSAppDbContext context,
            IHttpClientFactory httpClientFactory,
            ICashHandoverService cashHandoverService)
        {
            _orderService = orderService;
            _menuRepo = menuRepo;
            _context = context;
            _httpClientFactory = httpClientFactory;
            _cashHandoverService = cashHandoverService;
        }

        private async Task<string> GetUserBranchIdAsync()
        {
            var branchIdClaim = User.GetBranchId();
            if (!string.IsNullOrWhiteSpace(branchIdClaim))
            {
                return branchIdClaim;
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == userId);
                if (employee != null && !string.IsNullOrEmpty(employee.BranchId))
                {
                    return employee.BranchId;
                }
            }
            var firstBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Status == "Active");
            return firstBranch?.BranchId ?? "CB001";
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

        private async Task<(bool isEligible, string reasonCode, string message)> CheckCashierEligibilityAsync(int employeeId, string branchId)
        {
            var nowTime = DateTime.Now.TimeOfDay;
            if (nowTime < TimeSpan.FromHours(6))
            {
                return (false, "OutsideOperatingHours", "Hệ thống chỉ mở bán hàng từ 06:00 đến 24:00 hàng ngày. Hiện tại đang ngoài khung giờ phục vụ.");
            }

            if (User.IsInRole("RManager") || User.IsInRole("BranchManager"))
            {
                return (true, "Eligible", "Hợp lệ");
            }

            var result = await _cashHandoverService.CheckCashierSaleEligibilityAsync(employeeId, branchId);
            return (result.isEligible, result.reasonCode, result.message);
        }

        public async Task<IActionResult> CreateOrder()
        {
            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();
            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);
            if (!isEligible)
            {
                ViewBag.NotInShift = true;
                ViewBag.ReasonCode = reasonCode;
                ViewBag.NotInShiftMessage = message;

                if (reasonCode == "OutsideOperatingHours")
                {
                    ViewBag.RedirectAction = "Index";
                    ViewBag.RedirectController = "Home";
                    ViewBag.RedirectButtonText = "Về Trang Chủ";
                }
                else if (reasonCode == "NotScheduled" || reasonCode == "NotCheckedIn" || reasonCode == "ShiftEnded")
                {
                    if (reasonCode == "NotCheckedIn")
                    {
                        ViewBag.IsLogoutRequired = true;
                        ViewBag.RedirectButtonText = "Đăng xuất để Chấm công Face Login";
                    }
                    else
                    {
                        ViewBag.RedirectAction = "EmployeeIndex";
                        ViewBag.RedirectController = "WorkSchedule";
                        ViewBag.RedirectButtonText = "Xem lịch làm việc";
                    }
                }
                else if (reasonCode == "FirstShiftNotOpened")
                {
                    ViewBag.RedirectAction = "OpenShift";
                    ViewBag.RedirectController = "CashHandover";
                    ViewBag.RedirectButtonText = "Đến màn hình Mở ca";
                }
                else if (reasonCode == "MidShiftNotHandedOver")
                {
                    ViewBag.RedirectAction = "Handover";
                    ViewBag.RedirectController = "CashHandover";
                    ViewBag.RedirectButtonText = "Đến màn hình Nhận bàn giao";
                }
                else
                {
                    ViewBag.RedirectAction = "History";
                    ViewBag.RedirectController = "CashHandover";
                    ViewBag.RedirectButtonText = "Xem lịch sử giao ca";
                }
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

            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = message });
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

            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = message });
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
        public async Task<IActionResult> ProcessPayment(string orderId, string paymentMethod, decimal? customerCash = null, decimal? changeAmount = null, string? bankTransactionCode = null)
        {
            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();
            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = message });
            }

            bool result = await _orderService.ProcessPaymentAsync(orderId, paymentMethod, customerCash, changeAmount, bankTransactionCode);
            if (result) return Json(new { success = true });
            return BadRequest(new { success = false, message = "Thanh toán thất bại" });
        }

        [HttpPost]
        public async Task<IActionResult> ProcessRefund([FromBody] RefundRequestModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.OrderId))
            {
                return BadRequest(new { success = false, message = "Mã đơn hàng không hợp lệ." });
            }

            var cashierId = await GetUserCashierIdAsync();
            var branchId = await GetUserBranchIdAsync();

            // Enforce that only a cashier currently on an active, open shift can process refunds
            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = $"Chỉ thu ngân đang trong ca làm việc chính thức mới được phép thực hiện hoàn tiền! ({message})" });
            }

            bool result = await _orderService.ProcessRefundCashAsync(model.OrderId, model.RefundAmount, model.Reason ?? "Hoàn tiền do thiếu nguyên liệu/hủy món", cashierId);
            if (result)
            {
                return Json(new { success = true, message = "Đã hoàn tiền mặt thành công. Số tiền đã được trừ vào dòng tiền mặt của ca hiện tại." });
            }
            return BadRequest(new { success = false, message = "Không thể xử lý hoàn tiền cho đơn hàng này." });
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

            string txCode = "QR-MANUAL-" + DateTime.Now.ToString("HHmmss") + "-" + Random.Shared.Next(100, 999);
            order.PaymentStatus = "TransferSuccessPending";
            order.BankTransactionCode = txCode;
            order.PaymentMethod = $"Bank Transfer (Xác nhận thủ công #{txCode} - Đã nhận:{order.TotalAmount:N0}đ)";
            await _context.SaveChangesAsync();

            return Json(new { success = true, transactionCode = txCode });
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
                receivedAmount = receivedAmount,
                bankTransactionCode = order.BankTransactionCode
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
            var cashierId = await GetUserCashierIdAsync();
            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);

            ViewBag.CanRefund = isEligible;
            ViewBag.IneligibleReason = message;

            var vm = await _orderService.GetOrderHistoryAsync(branchId, status, fromDate, toDate, search, page);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(string orderId)
        {
            var details = await _orderService.GetOrderDetailsAsync(orderId);
            if (details == null) return NotFound();

            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();
            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);

            ViewBag.CanRefund = isEligible;
            ViewBag.IneligibleReason = message;

            return PartialView("_OrderDetailPartial", details);
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuForExchange()
        {
            var allCategories = await _menuRepo.GetProductCategoriesAsync();
            var allMasterProducts = await _menuRepo.GetMasterProductsAsync();

            var result = new
            {
                categories = allCategories.Select(c => new { categoryId = c.CategoryId, categoryName = c.CategoryName }),
                products = allMasterProducts.Where(m => m.ProductVariants != null && m.ProductVariants.Any()).Select(m => new
                {
                    productId = m.ProductId,
                    productName = m.ProductName ?? "",
                    categoryId = m.CategoryId,
                    imageUrl = m.ImageUrl ?? "",
                    variants = m.ProductVariants.Select(pv => new
                    {
                        variantId = pv.VariantId,
                        size = pv.SizeVariant ?? "S",
                        price = pv.SellingPrice
                    }).ToList()
                }).ToList()
            };
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> ExchangeOrder([FromBody] ExchangeOrderModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.OrderId) || model.Items == null || !model.Items.Any())
            {
                return BadRequest(new { success = false, message = "Dữ liệu đổi món không hợp lệ." });
            }

            var cashierId = await GetUserCashierIdAsync();
            var branchId = await GetUserBranchIdAsync();

            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = $"Chỉ thu ngân đang trong ca làm việc chính thức mới được phép sửa đơn / đổi món! ({message})" });
            }

            var result = await _orderService.ExchangeOrderItemsAsync(
                model.OrderId, 
                model.Items, 
                model.AdditionalPaymentMethod ?? "Cash", 
                model.CustomerCash, 
                model.ChangeAmount, 
                model.Reason, 
                cashierId);

            if (result.success)
            {
                return Json(new { 
                    success = true, 
                    message = result.message, 
                    refundDifference = result.refundDifference, 
                    additionalAmount = result.additionalAmount 
                });
            }

            return BadRequest(new { success = false, message = result.message });
        }
    }

    public class RefundRequestModel
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string? Reason { get; set; }
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
