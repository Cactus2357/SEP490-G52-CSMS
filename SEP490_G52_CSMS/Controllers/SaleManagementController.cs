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
        private readonly ICashierWorkEligibilityService _eligibilityService;
        private readonly IVoucherService _voucherService;

        public SaleManagementController(
            IOrderService orderService,
            IMenuRepository menuRepo,
            CSMSAppDbContext context,
            IHttpClientFactory httpClientFactory,
            ICashHandoverService cashHandoverService,
            ICashierWorkEligibilityService eligibilityService,
            IVoucherService voucherService)
        {
            _orderService = orderService;
            _menuRepo = menuRepo;
            _context = context;
            _httpClientFactory = httpClientFactory;
            _cashHandoverService = cashHandoverService;
            _eligibilityService = eligibilityService;
            _voucherService = voucherService;
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
            if (User.IsInRole("RManager") || User.IsInRole("BranchManager"))
            {
                return (true, "Eligible", "Hợp lệ");
            }

            var result = await _eligibilityService.CheckEligibilityAsync(employeeId, branchId);
            return (result.IsEligible, result.ReasonCode, result.Message);
        }

        [HttpGet("api/cashier/check-eligibility")]
        public async Task<IActionResult> GetCashierEligibility(int? cashierId, string? branchId)
        {
            var targetBranchId = !string.IsNullOrWhiteSpace(branchId) ? branchId : await GetUserBranchIdAsync();
            var targetCashierId = (cashierId.HasValue && cashierId.Value > 0) ? cashierId.Value : await GetUserCashierIdAsync();

            var result = await _eligibilityService.CheckEligibilityAsync(targetCashierId, targetBranchId);
            return Ok(result);
        }

        [HttpGet("api/sale/product-availability")]
        public async Task<IActionResult> GetProductAvailabilityStatus()
        {
            var branchId = await GetUserBranchIdAsync();
            var activeMenu = await _menuRepo.GetActiveMenuWithDetailsAsync(branchId);
            if (activeMenu?.MenuDetails == null)
            {
                return Ok(new { unavailableProductIds = new List<int>(), unavailableVariantIds = new List<int>() });
            }

            var unavailableVariants = activeMenu.MenuDetails.Where(md => !md.IsAvailable).ToList();
            var unavailableVariantIds = unavailableVariants.Select(md => md.VariantId).ToList();

            var groupedByProduct = activeMenu.MenuDetails
                .Where(md => md.ProductVariant != null)
                .GroupBy(md => md.ProductVariant!.ProductId)
                .ToList();

            var unavailableProductIds = new List<int>();
            foreach (var grp in groupedByProduct)
            {
                if (grp.All(md => !md.IsAvailable))
                {
                    unavailableProductIds.Add(grp.Key);
                }
            }

            return Ok(new { unavailableProductIds, unavailableVariantIds });
        }

        private async Task<(bool isValid, string? errorMessage)> ValidateCartItemsAvailabilityAsync(string branchId, IEnumerable<int> variantIds)
        {
            var activeMenu = await _menuRepo.GetActiveMenuWithDetailsAsync(branchId);
            if (activeMenu?.MenuDetails != null)
            {
                var unavailableDetails = activeMenu.MenuDetails
                    .Where(md => !md.IsAvailable)
                    .ToList();

                if (unavailableDetails.Any())
                {
                    var unavailableVariantIds = unavailableDetails.Select(md => md.VariantId).ToHashSet();
                    var offendingVariantId = variantIds.FirstOrDefault(vid => unavailableVariantIds.Contains(vid));
                    if (offendingVariantId > 0)
                    {
                        var offDetail = unavailableDetails.First(md => md.VariantId == offendingVariantId);
                        string prodName = offDetail.ProductVariant?.MasterProduct?.ProductName ?? "Sản phẩm";
                        string size = offDetail.ProductVariant?.SizeVariant ?? "";
                        return (false, $"Món [{prodName} ({size})] hiện đã tạm hết nguyên liệu (Bartender đã khóa món). Vui lòng chọn món khác hoặc xóa khỏi giỏ hàng!");
                    }
                }
            }
            return (true, null);
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

                var productMenuDetails = menu?.MenuDetails?
                    .Where(d => (d.ProductVariant != null && d.ProductVariant.ProductId == master.ProductId)
                             || master.ProductVariants.Any(pv => pv.VariantId == d.VariantId))
                    .ToList();

                bool hasProductDetails = productMenuDetails != null && productMenuDetails.Any();
                bool isProductGenerallyAvailable = !hasProductDetails || productMenuDetails!.Any(d => d.IsAvailable);

                var variants = master.ProductVariants.Select(pv =>
                {
                    var md = productMenuDetails?.FirstOrDefault(d => d.VariantId == pv.VariantId);
                    return new SaleVariantViewModel
                    {
                        VariantId = pv.VariantId,
                        SizeVariant = pv.SizeVariant ?? "S",
                        SellingPrice = pv.SellingPrice,
                        IsAvailable = md != null ? md.IsAvailable : isProductGenerallyAvailable
                    };
                }).ToList();

                var pvm = new SaleProductViewModel
                {
                    ProductId = master.ProductId,
                    ProductName = master.ProductName ?? "",
                    CategoryId = master.CategoryId,
                    CategoryName = master.ProductCategory?.CategoryName ?? "Other",
                    ImageUrl = master.ImageUrl ?? "",
                    Variants = variants,
                    IsAvailable = variants.Any(v => v.IsAvailable)
                };

                var def = pvm.Variants.FirstOrDefault(v => v.IsAvailable && v.SizeVariant == "S")
                          ?? pvm.Variants.FirstOrDefault(v => v.IsAvailable)
                          ?? pvm.Variants.FirstOrDefault();
                if (def != null)
                {
                    pvm.DefaultVariantId = def.VariantId;
                    pvm.DefaultPrice = def.SellingPrice;
                }
                menuProducts.Add(pvm);
            }

            vm.Categories = categories;
            vm.MenuProducts = menuProducts;

            // Load active vouchers for branch
            var activeVouchers = await _voucherService.GetBranchVouchersAsync(branchId, status: "Active");
            vm.AvailableVouchers = activeVouchers.Where(v => v.IsValidNow).ToList();

            // Load branch banking settings for QR transfer
            var branchSetting = await _context.BranchSettings.FirstOrDefaultAsync(s => s.BranchId == branchId);
            if (branchSetting == null)
            {
                branchSetting = new Models.Core.BranchSetting
                {
                    BranchId = branchId,
                    BankCode = "MBBank",
                    AccountNumber = "0333333333",
                    AccountName = "CSMS CAFE",
                    TransferPrefix = "CSMS"
                };
            }
            ViewBag.BranchSetting = branchSetting;

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

            var (isCartValid, availabilityError) = await ValidateCartItemsAvailabilityAsync(branchId, model.Items.Select(i => i.VariantId));
            if (!isCartValid)
            {
                return BadRequest(new { success = false, message = availabilityError });
            }

            var recipient = string.IsNullOrWhiteSpace(model.RecipientName) ? "Khách lẻ" : model.RecipientName.Trim();

            var orderItems = model.Items.Select(i => new OrderItem
            {
                VariantId = i.VariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList();

            var order = await _orderService.CreateOrderAsync(
                recipient,
                branchId,
                cashierId,
                orderItems,
                model.TableNumber,
                model.CustomerName,
                model.SubtotalAmount,
                model.DiscountAmount,
                model.TradeDiscountAmount,
                model.OrderNotes,
                model.VoucherCode);

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
            try
            {
                if (model == null || model.Items == null || !model.Items.Any())
                {
                    return BadRequest(new { success = false, message = "Dữ liệu đơn hàng không hợp lệ (giỏ hàng rỗng)." });
                }

                var branchId = await GetUserBranchIdAsync();
                var cashierId = await GetUserCashierIdAsync();

                var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);
                if (!isEligible)
                {
                    return BadRequest(new { success = false, message = message });
                }

                var (isCartValid, availabilityError) = await ValidateCartItemsAvailabilityAsync(branchId, model.Items.Select(i => i.VariantId));
                if (!isCartValid)
                {
                    return BadRequest(new { success = false, message = availabilityError });
                }

                var recipient = string.IsNullOrWhiteSpace(model.RecipientName) ? "Khách lẻ" : model.RecipientName.Trim();

                var orderItems = model.Items.Select(i => new OrderItem
                {
                    VariantId = i.VariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList();

                var order = await _orderService.CreateOrderAsync(
                    recipient,
                    branchId,
                    cashierId,
                    orderItems,
                    model.TableNumber,
                    model.CustomerName,
                    model.SubtotalAmount,
                    model.DiscountAmount,
                    model.TradeDiscountAmount,
                    model.OrderNotes,
                    model.VoucherCode);

                if (order == null || string.IsNullOrEmpty(order.OrderId))
                {
                    return BadRequest(new { success = false, message = "Không thể khởi tạo đơn hàng." });
                }

                if (model.InitialCashAmount.HasValue && model.InitialCashAmount.Value > 0)
                {
                    var initialCash = model.InitialCashAmount.Value;
                    var payment = new Payment
                    {
                        PaymentId = $"PAY-CSH-{order.OrderId}-{DateTime.UtcNow:HHmmss}-{Random.Shared.Next(100, 999)}",
                        OrderId = order.OrderId,
                        BranchId = branchId,
                        CashierId = cashierId,
                        PaymentType = "Payment",
                        PaymentMethod = "Cash",
                        Amount = initialCash,
                        CustomerCash = initialCash,
                        ChangeAmount = 0,
                        Status = "Success",
                        Notes = "Thanh toán tiền mặt 1 phần trước khi chuyển khoản nốt",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Payments.AddAsync(payment);

                    order.PaymentMethod = "Split";
                    order.PaymentStatus = "PartiallyPaid";
                }
                else
                {
                    order.PaymentMethod = "Bank Transfer";
                    order.PaymentStatus = "Unpaid";
                }
                await _context.SaveChangesAsync();

                return Json(new { success = true, orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReceiptData(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return BadRequest(new { success = false, message = "Mã đơn hàng không hợp lệ." });
            }

            var detail = await _orderService.GetOrderDetailsAsync(orderId.Trim());
            if (detail == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            return Json(new
            {
                success = true,
                orderId = detail.OrderId,
                branchName = detail.BranchName,
                branchAddress = detail.BranchAddress,
                branchPhone = detail.BranchPhone,
                cashierName = detail.CashierName,
                orderTime = detail.OrderTime.ToVietnamTimeString("dd/MM/yyyy HH:mm"),
                tableNumber = detail.TableNumber,
                customerName = detail.CustomerName,
                orderNotes = detail.OrderNotes,
                items = detail.Items.Select(i => new
                {
                    productName = i.ProductName,
                    size = i.Size,
                    quantity = i.Quantity,
                    unitPrice = i.UnitPrice,
                    amount = i.Amount
                }),
                subtotalAmount = detail.SubtotalAmount,
                discountAmount = detail.DiscountAmount,
                tradeDiscountAmount = detail.TradeDiscountAmount,
                voucherCode = detail.VoucherCode,
                totalAmount = detail.TotalAmount,
                paymentMethod = detail.PaymentMethod,
                cashAmount = detail.CashAmount,
                bankAmount = detail.BankAmount
            });
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
            var order = await _context.Orders.Include(o => o.Payments).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound(new { success = false, message = "Order not found" });

            decimal transferRemaining = Math.Max(0, order.TotalAmount - order.CashPaid);
            if (transferRemaining <= 0) transferRemaining = order.TotalAmount;

            string txCode = "QR-MANUAL-" + DateTime.UtcNow.ToString("HHmmss") + "-" + Random.Shared.Next(100, 999);
            order.PaymentStatus = "TransferSuccessPending";
            order.PaymentMethod = order.CashPaid > 0 ? $"Split (CK:{transferRemaining:N0}đ + TM:{order.CashPaid:N0}đ)" : $"Bank Transfer (Xác nhận thủ công #{txCode} - Đã nhận:{transferRemaining:N0}đ)";

            var cashierId = await GetUserCashierIdAsync();
            var branchId = await GetUserBranchIdAsync();

            var payment = new Payment
            {
                PaymentId = $"PAY-BNK-{order.OrderId}-{DateTime.UtcNow:HHmmss}-{Random.Shared.Next(100, 999)}",
                OrderId = order.OrderId,
                BranchId = branchId,
                CashierId = cashierId,
                PaymentType = "Payment",
                PaymentMethod = "BankTransfer",
                Amount = transferRemaining,
                TransactionCode = txCode,
                Status = "Success",
                Notes = order.PaymentMethod,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Payments.AddAsync(payment);
            order.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return Json(new { success = true, transactionCode = txCode });
        }

        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(string orderId)
        {
            DbInitializer.EnsureTablesCreated(_context);
            var order = await _context.Orders.Include(o => o.Payments).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound(new { success = false, message = "Order not found" });

            decimal receivedBankAmount = order.BankPaid;
            decimal receivedCashAmount = order.CashPaid;
            if (receivedBankAmount == 0 && !string.IsNullOrEmpty(order.PaymentMethod) && order.PaymentMethod.Contains("Đã nhận:"))
            {
                var match = Regex.Match(order.PaymentMethod, @"Đã nhận:([\d\.,]+)đ?");
                if (match.Success)
                {
                    string numStr = match.Groups[1].Value.Replace(".", "").Replace(",", "");
                    if (decimal.TryParse(numStr, out decimal parsed))
                    {
                        receivedBankAmount = parsed;
                    }
                }
            }

            decimal remainingAmount = Math.Max(0, order.TotalAmount - receivedBankAmount - receivedCashAmount);

            return Json(new
            {
                success = true,
                paymentStatus = order.PaymentStatus,
                totalAmount = order.TotalAmount,
                receivedAmount = (receivedBankAmount + receivedCashAmount) > 0 ? (receivedBankAmount + receivedCashAmount) : order.TotalAmount,
                bankAmount = receivedBankAmount,
                cashAmount = receivedCashAmount,
                remainingAmount = remainingAmount,
                bankTransactionCode = order.LatestBankTransactionCode
            });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteSplitPayment([FromBody] SplitPaymentSubmissionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.OrderId))
            {
                return BadRequest(new { success = false, message = "Dữ liệu thanh toán không hợp lệ." });
            }

            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();
            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = message });
            }

            var order = await _context.Orders.Include(o => o.Payments).FirstOrDefaultAsync(o => o.OrderId == model.OrderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            if (order.PaymentStatus == "Paid" || order.PaymentStatus == "Completed")
            {
                return BadRequest(new { success = false, message = "Đơn hàng này đã được thanh toán hoàn tất." });
            }

            decimal bankAmount = model.BankAmount > 0 ? model.BankAmount : order.BankPaid;
            decimal cashAmount = model.CashAmount > 0 ? model.CashAmount : Math.Max(0, order.TotalAmount - bankAmount);

            bool result = await _orderService.ProcessSplitPaymentAsync(
                model.OrderId,
                cashAmount,
                bankAmount,
                model.CustomerCash,
                model.ChangeAmount,
                model.BankTransactionCode ?? order.LatestBankTransactionCode
            );

            if (result)
            {
                return Json(new
                {
                    success = true,
                    orderId = order.OrderId,
                    cashAmount = cashAmount,
                    bankAmount = bankAmount,
                    message = "Thanh toán kết hợp thành công!"
                });
            }

            return BadRequest(new { success = false, message = "Không thể xử lý thanh toán kết hợp." });
        }

        [HttpPost]
        public async Task<IActionResult> SimulatePartialTransfer(string orderId, decimal amount)
        {
            var order = await _context.Orders.Include(o => o.Payments).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound(new { success = false, message = "Order not found" });

            string txCode = "MBB-" + DateTime.Now.ToString("HHmmss");
            var cashierId = await GetUserCashierIdAsync();
            var branchId = await GetUserBranchIdAsync();

            var payment = new Payment
            {
                PaymentId = $"PAY-BNK-{order.OrderId}-{DateTime.UtcNow:HHmmss}-{Random.Shared.Next(100, 999)}",
                OrderId = order.OrderId,
                BranchId = branchId,
                CashierId = cashierId,
                PaymentType = "Payment",
                PaymentMethod = "BankTransfer",
                Amount = amount,
                TransactionCode = txCode,
                Status = "Success",
                Notes = $"Bank Transfer mô phỏng #{txCode}",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Payments.AddAsync(payment);
            order.Payments.Add(payment);

            decimal totalBank = order.BankPaid;
            decimal totalPaid = order.PaidAmount;

            if (totalPaid >= (order.TotalAmount - 1))
            {
                order.PaymentStatus = "TransferSuccessPending";
                order.PaymentMethod = $"Bank Transfer (Xác nhận thủ công #{txCode} - Đã nhận:{totalBank:N0}đ)";
            }
            else
            {
                order.PaymentStatus = "PartiallyPaid";
                order.PaymentMethod = $"Bank Transfer 1 phần (Xác nhận thủ công #{txCode} - Đã nhận:{totalBank:N0}đ)";
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, bankAmount = totalBank, remainingAmount = Math.Max(0, order.TotalAmount - totalPaid) });
        }

        [HttpPost]
        public async Task<IActionResult> CancelBankTransfer(string orderId)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                var order = await _context.Orders
                    .Include(o => o.Payments)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order != null)
                {
                    var now = order.CreatedAt.Kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;
                    if ((now - order.CreatedAt).TotalHours > 24)
                    {
                        return BadRequest(new { success = false, message = "Không thể hủy đơn hàng đã tạo quá 1 ngày (quá 24 giờ)." });
                    }

                    if (order.PaymentStatus == "Paid" || order.PaymentStatus == "Completed")
                    {
                        return BadRequest(new { success = false, message = "Đơn hàng đã thanh toán hoàn tất. Không thể hủy giao dịch chuyển khoản!" });
                    }

                    if (order.BankPaid > 0)
                    {
                        return BadRequest(new { success = false, message = $"Đơn hàng đã nhận {order.BankPaid:N0}đ chuyển khoản từ ngân hàng của khách. Vui lòng thu nốt tiền mặt phần còn thiếu hoặc xử lý qua quản lý!" });
                    }

                    // Xóa các giao dịch thanh toán dở dang (ví dụ tiền mặt thu trước khi chuyển khoản nốt)
                    if (order.Payments != null && order.Payments.Any())
                    {
                        _context.Payments.RemoveRange(order.Payments);
                    }

                    if (order.OrderItems != null && order.OrderItems.Any())
                    {
                        _context.OrderItems.RemoveRange(order.OrderItems);
                    }

                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                }
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> CancelUnpaidOrder(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return BadRequest(new { success = false, message = "Mã đơn hàng không hợp lệ." });
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            var now = order.CreatedAt.Kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;
            if ((now - order.CreatedAt).TotalHours > 24)
            {
                return BadRequest(new { success = false, message = "Không thể hủy đơn hàng đã tạo quá 1 ngày (quá 24 giờ)." });
            }

            if (order.PaymentStatus != "Unpaid")
            {
                return BadRequest(new { success = false, message = "Chỉ có thể hủy trực tiếp các đơn chưa thanh toán." });
            }

            order.PaymentStatus = "Cancelled";
            order.BrewingStatus = "Cancelled";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã hủy đơn hàng chưa thanh toán." });
        }

        public async Task<IActionResult> OrderHistory(string status, DateTime? fromDate, DateTime? toDate, string search, int page = 1)
        {
            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();
            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);

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
                return Json(new
                {
                    success = true,
                    message = result.message,
                    additionalAmount = result.additionalAmount
                });
            }

            return BadRequest(new { success = false, message = result.message });
        }

        [HttpPost]
        public async Task<IActionResult> CheckVoucher([FromBody] CheckVoucherRequestModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.VoucherCode))
            {
                return BadRequest(new { success = false, message = "Mã giảm giá không tồn tại" });
            }

            var branchId = await GetUserBranchIdAsync();
            var (success, message, voucher, percent, discountAmount) = await _voucherService.ValidateVoucherForOrderAsync(model.VoucherCode, branchId, model.Subtotal);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Json(new
            {
                success = true,
                message,
                voucherId = voucher?.VoucherId,
                voucherCode = voucher?.VoucherCode,
                discountPercent = percent,
                discountAmount = discountAmount,
                description = voucher?.Description,
                remainingQuantity = voucher?.RemainingQuantity
            });
        }

        [HttpPost]
        public async Task<IActionResult> MarkOrderAsDelivered(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return BadRequest(new { success = false, message = "Mã đơn hàng không hợp lệ." });

            var branchId = await GetUserBranchIdAsync();
            var cashierId = await GetUserCashierIdAsync();
            var (isEligible, reasonCode, message) = await CheckCashierEligibilityAsync(cashierId, branchId);
            if (!isEligible)
                return BadRequest(new { success = false, message = message });

            var result = await _orderService.MarkOrderAsDeliveredAsync(orderId.Trim());
            if (result.success)
                return Json(new { success = true, message = result.message });

            return BadRequest(new { success = false, message = result.message });
        }
    }

    public class CheckVoucherRequestModel
    {
        public string VoucherCode { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
    }

    public class OrderSubmissionModel
    {
        public string RecipientName { get; set; } = "";
        public string? TableNumber { get; set; }
        public string? CustomerName { get; set; }
        public decimal? SubtotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TradeDiscountAmount { get; set; }
        public string? VoucherCode { get; set; }
        public string? OrderNotes { get; set; }
        public string? IdempotencyKey { get; set; }
        public decimal? InitialCashAmount { get; set; }
        public List<OrderItemSubmission> Items { get; set; } = new List<OrderItemSubmission>();
    }

    public class CashPaymentSubmissionModel : OrderSubmissionModel
    {
        public decimal CustomerCash { get; set; }
        public decimal ChangeAmount { get; set; }
    }

    public class SplitPaymentSubmissionModel
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal CashAmount { get; set; }
        public decimal BankAmount { get; set; }
        public decimal? CustomerCash { get; set; }
        public decimal? ChangeAmount { get; set; }
        public string? BankTransactionCode { get; set; }
    }

    public class OrderItemSubmission
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
