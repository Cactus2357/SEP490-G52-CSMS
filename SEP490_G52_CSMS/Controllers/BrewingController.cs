using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize]
    public class BrewingController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly CSMSAppDbContext _context;

        public BrewingController(IOrderService orderService, CSMSAppDbContext context)
        {
            _orderService = orderService;
            _context = context;
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
                if (employee != null && !string.IsNullOrWhiteSpace(employee.BranchId))
                {
                    return employee.BranchId;
                }
            }
            return "";
        }

        private int GetUserId()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id);
            return id;
        }

        public async Task<IActionResult> Index()
        {
            var branchId = await GetUserBranchIdAsync();
            var waitingAndBrewingOrders = await _orderService.GetWaitingAndBrewingOrdersAsync(branchId);
            return View(waitingAndBrewingOrders);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(string orderId)
        {
            var details = await _orderService.GetOrderDetailsAsync(orderId);
            if (details == null) return NotFound();

            return PartialView("_BrewingOrderDetailPartial", details);
        }

        [HttpGet]
        public async Task<IActionResult> GetLiveOrders()
        {
            var branchId = await GetUserBranchIdAsync();
            var orders = await _orderService.GetWaitingAndBrewingOrdersAsync(branchId);
            var result = orders.Select(o => new
            {
                orderId = o.OrderId,
                recipientName = string.IsNullOrWhiteSpace(o.RecipientName) ? "Khách lẻ" : o.RecipientName,
                orderTime = o.OrderTime.ToString("dd/MM/yyyy HH:mm"),
                brewingStatus = o.BrewingStatus,
                displayStatus = o.DisplayStatus,
                items = o.Items.Select((item, index) => new
                {
                    stt = index + 1,
                    variantId = item.VariantId,
                    productName = item.ProductName,
                    size = item.Size,
                    quantity = item.Quantity,
                    unitPrice = item.UnitPrice,
                    amount = item.Amount
                })
            });
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> StartBrewing(string orderId)
        {
            var result = await _orderService.StartBrewingAsync(orderId);
            if (result) return Json(new { success = true });
            return BadRequest(new { success = false, message = "Không thể bắt đầu pha chế đơn hàng này." });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteBrewing(string orderId)
        {
            var result = await _orderService.CompleteBrewingAsync(orderId);
            if (result) return Json(new { success = true });
            return BadRequest(new { success = false, message = "Không thể hoàn thành đơn hàng này." });
        }

        [HttpPost]
        public async Task<IActionResult> ReportMissingIngredients([FromBody] MissingIngredientsReportModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.OrderId))
            {
                return BadRequest(new { success = false, message = "Mã đơn hàng không hợp lệ." });
            }

            if (model.MissingVariantIds == null || !model.MissingVariantIds.Any())
            {
                return BadRequest(new { success = false, message = "Vui lòng chọn ít nhất một món bị thiếu nguyên liệu." });
            }

            int userId = GetUserId();
            var ok = await _orderService.ReportMissingIngredientsAsync(model.OrderId, model.MissingVariantIds, model.Reason, userId);

            if (ok)
            {
                return Json(new { 
                    success = true, 
                    message = $"Đã ghi nhận báo thiếu nguyên liệu cho đơn {model.OrderId} và gửi thông báo tới Thu ngân." 
                });
            }

            return BadRequest(new { success = false, message = "Không thể gửi báo cáo thiếu nguyên liệu cho đơn hàng này." });
        }
    }
}
