using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Services;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "BranchManager,RManager")]
    public class OrderManagementController : Controller
    {
        private readonly IOrderManagementService _orderManagementService;
        private readonly CSMSAppDbContext _context;

        public OrderManagementController(IOrderManagementService orderManagementService, CSMSAppDbContext context)
        {
            _orderManagementService = orderManagementService;
            _context = context;
        }

        private async Task<(string BranchId, string BranchName)> GetUserBranchAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employee = await _context.Employees.Include(e => e.Branch).FirstOrDefaultAsync(e => e.EmployeeId == userId);
                if (employee != null && employee.Branch != null)
                {
                    return (employee.BranchId ?? "", employee.Branch.BranchName ?? "");
                }
            }
            return ("", "");
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string searchCashier = "", string status = "Tất cả", string? cursor = null, string direction = "next")
        {
            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId) && !User.IsInRole("RManager")) 
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Content($"Không thể xác định được chi nhánh của bạn. UserIdStr: {userIdStr}");
            }

            if (string.IsNullOrEmpty(branchName))
            {
                branchName = "Toàn hệ thống (RManager)";
            }

            var model = await _orderManagementService.GetOrderManagementListAsync(branchId, branchName, searchCashier, status, fromDate, toDate, cursor, direction, 10);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string orderId, bool isInline = false)
        {
            var (branchId, _) = await GetUserBranchAsync();
            var model = await _orderManagementService.GetOrderDetailAsync(orderId, branchId);
            
            if (model == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            if (isInline)
            {
                return PartialView("_OrderDetailInline", model);
            }

            return PartialView("_OrderDetailModal", model);
        }
    }
}
