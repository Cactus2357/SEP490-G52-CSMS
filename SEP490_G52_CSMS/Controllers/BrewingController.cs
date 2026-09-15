using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "Bartender,Barista,BranchManager,RManager,Cashier")]
    public class BrewingController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IMenuService _menuService;
        private readonly CSMSAppDbContext _context;

        public BrewingController(IOrderService orderService, IMenuService menuService, CSMSAppDbContext context)
        {
            _orderService = orderService;
            _menuService = menuService;
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
            var defaultBranch = await _context.Branches.Select(b => b.BranchId).FirstOrDefaultAsync();
            return defaultBranch ?? "CB001";
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int id);
            return id;
        }

        private async Task<(bool isEligible, string reasonCode, string message)> CheckBartenderEligibilityAsync(int bartenderId, string branchId)
        {
            if (User.IsInRole("RManager") || User.IsInRole("BranchManager") || User.IsInRole("Cashier"))
            {
                return (true, "Eligible", "Hợp lệ");
            }

            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var nowTime = DateTime.Now.TimeOfDay;

            // Tìm lịch phân công trực ca của nhân viên tại chi nhánh hôm nay
            var rosters = await _context.WeeklyRosterGrids
                .Include(r => r.FixedShift)
                .Include(r => r.AttendanceLogs)
                .Where(r => r.EmployeeId == bartenderId && r.BranchId == branchId && (r.AssignmentDate.Date == today || r.AssignmentDate.Date == yesterday))
                .ToListAsync();

            if (!rosters.Any())
            {
                return (false, "NotScheduled", "Bạn không có lịch phân công ca làm việc tại chi nhánh hôm nay. Vui lòng kiểm tra lại Lịch làm việc.");
            }

            // Tìm ca phù hợp với khung giờ hiện tại
            var matchingRoster = rosters.FirstOrDefault(r =>
            {
                if (r.FixedShift == null) return false;
                var start = r.FixedShift.StartTime;
                var end = r.FixedShift.EndTime;
                var earlyWindow = TimeSpan.FromHours(1);

                if (start <= end)
                {
                    return r.AssignmentDate.Date == today && nowTime >= start.Subtract(earlyWindow) && nowTime <= end.Add(TimeSpan.FromHours(1));
                }
                else
                {
                    if (r.AssignmentDate.Date == today) return nowTime >= start.Subtract(earlyWindow);
                    if (r.AssignmentDate.Date == yesterday) return nowTime <= end.Add(TimeSpan.FromHours(1));
                    return false;
                }
            }) ?? rosters.OrderBy(r => r.FixedShift?.StartTime).FirstOrDefault();

            if (matchingRoster == null || matchingRoster.FixedShift == null)
            {
                return (false, "NotScheduled", "Hiện tại chưa tới giờ ca làm việc của bạn. Vui lòng kiểm tra lại Lịch làm việc.");
            }

            var log = matchingRoster.AttendanceLogs.FirstOrDefault();
            if (log == null || log.CheckInTime == null || (log.OverallStatus != "Present" && log.CheckInStatus == "Absent"))
            {
                return (false, "NotCheckedIn", $"Bạn chưa thực hiện chấm công vào ca {matchingRoster.FixedShift.ShiftName}. Vui lòng chấm công trước khi vào màn hình pha chế.");
            }

            if (log.CheckOutTime != null || log.CheckOutStatus == "CheckedOut")
            {
                return (false, "ShiftEnded", $"Ca làm việc {matchingRoster.FixedShift.ShiftName} của bạn đã kết thúc (đã chấm công ra ca).");
            }

            return (true, "Eligible", "Hợp lệ");
        }

        [Authorize(Roles = "Bartender,Barista,BranchManager,RManager")]
        public async Task<IActionResult> Index()
        {
            var branchId = await GetUserBranchIdAsync();
            var userId = GetUserId();

            var (isEligible, reasonCode, message) = await CheckBartenderEligibilityAsync(userId, branchId);
            if (!isEligible)
            {
                ViewBag.NotInShift = true;
                ViewBag.ReasonCode = reasonCode;
                ViewBag.NotInShiftMessage = message;
                return View(Enumerable.Empty<OrderSummaryViewModel>());
            }

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
            var userId = GetUserId();

            var (isEligible, reasonCode, message) = await CheckBartenderEligibilityAsync(userId, branchId);
            if (!isEligible)
            {
                return Json(new List<object>());
            }

            var orders = await _orderService.GetWaitingAndBrewingOrdersAsync(branchId);
            var result = orders.Select(o => new
            {
                orderId = o.OrderId,
                recipientName = string.IsNullOrWhiteSpace(o.RecipientName) ? "Khách lẻ" : o.RecipientName,
                tableNumber = o.TableNumber,
                customerName = o.CustomerName,
                orderNotes = o.OrderNotes,
                orderTime = o.OrderTime.ToString("dd/MM/yyyy HH:mm"),
                orderTimeRaw = o.OrderTime.ToString("o"),
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
                    amount = item.Amount,
                    isCompleted = item.IsCompleted
                })
            });
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleOrderItem(string orderId, int variantId, bool? isCompleted)
        {
            var branchId = await GetUserBranchIdAsync();
            var (isEligible, reasonCode, message) = await CheckBartenderEligibilityAsync(GetUserId(), branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = message });
            }

            var result = await _orderService.ToggleOrderItemBrewingAsync(orderId, variantId, isCompleted);
            if (result.Success)
            {
                return Json(new
                {
                    success = true,
                    message = result.Message,
                    orderCompleted = result.OrderCompleted,
                    completedItems = result.CompletedItems,
                    totalItems = result.TotalItems
                });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> StartBrewing(string orderId)
        {
            var branchId = await GetUserBranchIdAsync();
            var (isEligible, reasonCode, message) = await CheckBartenderEligibilityAsync(GetUserId(), branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = message });
            }

            var result = await _orderService.StartBrewingAsync(orderId);
            if (result) return Json(new { success = true });
            return BadRequest(new { success = false, message = "Không thể bắt đầu pha chế đơn hàng này (đơn có thể không tồn tại hoặc đã tạo quá 1 ngày)." });
        }

        [HttpPost]
        public async Task<IActionResult> StartBrewingMultiple([FromBody] List<string> orderIds)
        {
            if (orderIds == null || !orderIds.Any())
            {
                return BadRequest(new { success = false, message = "Không có đơn hàng nào được chọn." });
            }

            var branchId = await GetUserBranchIdAsync();
            var (isEligible, reasonCode, message) = await CheckBartenderEligibilityAsync(GetUserId(), branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = message });
            }

            int successCount = 0;
            foreach (var id in orderIds)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    var ok = await _orderService.StartBrewingAsync(id.Trim());
                    if (ok) successCount++;
                }
            }

            return Json(new { success = true, count = successCount, message = $"Đã bắt đầu pha chế {successCount} đơn hàng." });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteBrewing(string orderId)
        {
            var branchId = await GetUserBranchIdAsync();
            var (isEligible, reasonCode, message) = await CheckBartenderEligibilityAsync(GetUserId(), branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = message });
            }

            var result = await _orderService.CompleteBrewingAsync(orderId);
            if (result) return Json(new { success = true });
            return BadRequest(new { success = false, message = "Không thể hoàn thành đơn hàng này (đơn có thể không tồn tại hoặc đã tạo quá 1 ngày)." });
        }

        [HttpPost]
        public async Task<IActionResult> ReportMissingIngredients([FromBody] MissingIngredientsReportModel model)
        {
            return BadRequest(new { success = false, message = "Chức năng báo thiếu nguyên liệu từ pha chế đã ngừng hỗ trợ." });
        }



        [HttpGet]
        public async Task<IActionResult> ProductAvailability(string? search, int? categoryId, string? status)
        {
            var branchId = await GetUserBranchIdAsync();
            var userId = GetUserId();

            var (isEligible, reasonCode, message) = await CheckBartenderEligibilityAsync(userId, branchId);
            if (!isEligible)
            {
                ViewBag.NotInShift = true;
                ViewBag.ReasonCode = reasonCode;
                ViewBag.NotInShiftMessage = message;
                return View(new BartenderProductAvailabilityViewModel { BranchId = branchId });
            }

            var vm = await _menuService.GetBranchProductAvailabilityAsync(branchId, search, categoryId, status);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleProductAvailability(int productId)
        {
            var branchId = await GetUserBranchIdAsync();
            var userId = GetUserId();

            var (isEligible, reasonCode, message) = await CheckBartenderEligibilityAsync(userId, branchId);
            if (!isEligible)
            {
                return BadRequest(new { success = false, message = message });
            }

            var (success, msg, newState) = await _menuService.ToggleProductAvailabilityAsync(branchId, productId, userId);
            if (success)
            {
                return Json(new { success = true, message = msg, newState = newState });
            }

            return BadRequest(new { success = false, message = msg });
        }
    }
}
