using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly CSMSAppDbContext _context;

        public NotificationController(CSMSAppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            int.TryParse(userIdStr, out int userId);

            var query = _context.Notifications
                .Where(n => !n.IsRead && (n.RecipientUserId == null || n.RecipientUserId == userId || n.RecipientRole == userRole));

            var unreadNotifications = await query.ToListAsync();
            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
