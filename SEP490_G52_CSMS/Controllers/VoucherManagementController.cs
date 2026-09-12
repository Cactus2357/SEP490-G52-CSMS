using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "BranchManager,RManager")]
    public class VoucherManagementController : Controller
    {
        private readonly IVoucherService _voucherService;
        private readonly CSMSAppDbContext _context;

        public VoucherManagementController(IVoucherService voucherService, CSMSAppDbContext context)
        {
            _voucherService = voucherService;
            _context = context;
        }

        private async Task<(string BranchId, string BranchName, int UserId)> GetCurrentBranchAndUserAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employee = await _context.Employees.Include(e => e.Branch).FirstOrDefaultAsync(e => e.EmployeeId == userId);
                if (employee != null)
                {
                    return (employee.BranchId ?? "", employee.Branch?.BranchName ?? "Chi nhánh", userId);
                }
            }
            return ("", "", 0);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? status = null, string? search = null)
        {
            var (branchId, branchName, _) = await GetCurrentBranchAndUserAsync();
            if (string.IsNullOrEmpty(branchId) && !User.IsInRole("RManager"))
            {
                return Content("Không thể xác định chi nhánh của bạn.");
            }

            var allVouchers = await _voucherService.GetBranchVouchersAsync(branchId, status: null, search: null);
            var filteredVouchers = await _voucherService.GetBranchVouchersAsync(branchId, status, search);

            var now = DateTime.Now;
            var vm = new VoucherIndexViewModel
            {
                BranchId = branchId,
                BranchName = string.IsNullOrEmpty(branchName) ? "Toàn chuỗi" : branchName,
                StatusFilter = status ?? "Tất cả",
                SearchTerm = search ?? "",
                TotalCount = allVouchers.Count,
                ActiveCount = allVouchers.Count(v => v.IsActive && now >= v.StartDate && now <= v.EndDate && v.UsedCount < v.Quantity),
                ExpiredOrUsedUpCount = allVouchers.Count(v => now > v.EndDate || v.UsedCount >= v.Quantity),
                Vouchers = filteredVouchers
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVoucherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ.";
                return BadRequest(new { success = false, message = firstError });
            }

            var (branchId, _, userId) = await GetCurrentBranchAndUserAsync();
            if (string.IsNullOrEmpty(branchId) && !User.IsInRole("RManager"))
            {
                return BadRequest(new { success = false, message = "Không xác định được chi nhánh của bạn." });
            }

            var voucher = new Voucher
            {
                VoucherCode = model.VoucherCode,
                BranchId = string.IsNullOrEmpty(branchId) ? null : branchId,
                DiscountPercent = model.DiscountPercent,
                Quantity = model.Quantity,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Description = model.Description
            };

            var (success, message) = await _voucherService.CreateVoucherAsync(voucher, userId);
            if (!success)
            {
                return BadRequest(new { success = false, message = message });
            }

            return Json(new { success = true, message = message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int voucherId)
        {
            var (branchId, _, _) = await GetCurrentBranchAndUserAsync();
            bool result = await _voucherService.ToggleVoucherStatusAsync(voucherId, branchId);
            if (!result)
            {
                return BadRequest(new { success = false, message = "Không tìm thấy voucher hoặc không có quyền thao tác." });
            }

            return Json(new { success = true, message = "Cập nhật trạng thái voucher thành công." });
        }

        [HttpGet]
        public async Task<IActionResult> GetVoucherOrders(int voucherId)
        {
            try
            {
                var (branchId, _, _) = await GetCurrentBranchAndUserAsync();
                var orders = await _voucherService.GetVoucherOrderHistoryAsync(voucherId, branchId);

                var list = orders.Select(o => new VoucherOrderItemViewModel
                {
                    OrderId = o.OrderId ?? "",
                    CreatedAt = o.CreatedAt,
                    CustomerName = !string.IsNullOrWhiteSpace(o.CustomerName) ? o.CustomerName : (!string.IsNullOrWhiteSpace(o.RecipientName) ? o.RecipientName : "Khách lẻ"),
                    TableNumber = !string.IsNullOrWhiteSpace(o.TableNumber) ? o.TableNumber : "Mang về",
                    SubtotalAmount = o.SubtotalAmount > 0 ? o.SubtotalAmount : o.TotalAmount,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    CashierName = o.Cashier?.FullName ?? (o.CashierId > 0 ? $"Thu ngân #{o.CashierId}" : "—"),
                    PaymentStatus = o.PaymentStatus ?? ""
                }).ToList();

                return Json(new { success = true, orders = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tải lịch sử đơn hàng: " + ex.Message, orders = new List<VoucherOrderItemViewModel>() });
            }
        }
    }
}
