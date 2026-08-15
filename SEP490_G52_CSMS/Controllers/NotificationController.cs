using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly CSMSAppDbContext _context;

        public NotificationController(INotificationService notificationService, CSMSAppDbContext context)
        {
            _notificationService = notificationService;
            _context = context;
        }

        private async Task<string?> GetUserBranchIdAsync()
        {
            var branchIdClaim = User.GetBranchId();
            if (!string.IsNullOrWhiteSpace(branchIdClaim))
            {
                return branchIdClaim;
            }

            int userId = GetUserId();
            if (userId > 0)
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == userId);
                if (employee != null && !string.IsNullOrWhiteSpace(employee.BranchId))
                {
                    return employee.BranchId;
                }
            }
            return null;
        }

        /// <summary>Marks a single notification as read (called when user clicks an item).</summary>
        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();
            var branchId = await GetUserBranchIdAsync();
            var ok = await _notificationService.MarkReadAsync(id, userId, userRole, branchId);
            return Json(new { success = ok });
        }

        /// <summary>Marks ALL unread notifications as read for the current user (clear all).</summary>
        [HttpPost]
        public async Task<IActionResult> ClearAll()
        {
            var branchId = await GetUserBranchIdAsync();
            await _notificationService.ClearAllAsync(GetUserId(), GetUserRole(), branchId);
            return Json(new { success = true });
        }

        /// <summary>Returns all notifications (read + unread) as JSON for the full-history modal.</summary>
        [HttpGet]
        public async Task<IActionResult> All()
        {
            var userId = GetUserId();
            var userRole = GetUserRole();
            var branchId = await GetUserBranchIdAsync();
            var list = await _notificationService.GetAllForUserAsync(userId, userRole, branchId);

            var result = list.ConvertAll(n => new
            {
                n.NotificationId,
                n.Title,
                n.Message,
                CreatedTime = n.CreatedTime.ToString("HH:mm dd/MM/yyyy"),
                n.IsRead,
                n.ResourceUrl
            });

            return Json(result);
        }

        // ── helpers ──────────────────────────────────────────────────────────
        private int GetUserId()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id);
            return id;
        }

        private string GetUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
