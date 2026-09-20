using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class NotificationService : INotificationService
    {
        private readonly CSMSAppDbContext _context;

        public NotificationService(CSMSAppDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task SendAsync(NotificationEvent evt)
        {
            var notification = new Notification
            {
                Title = evt.Title,
                Message = evt.Message,
                CreatedTime = DateTime.UtcNow,
                IsRead = false,
                RecipientUserId = evt.RecipientUserId,
                RecipientRole = evt.RecipientRole,
                ResourceUrl = evt.ResourceUrl,
                BranchId = evt.BranchId
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public static bool IsRoleMatch(string? recipientRole, string userRole)
        {
            if (string.IsNullOrWhiteSpace(recipientRole)) return true; // Broadcast / system announcement
            if (string.Equals(recipientRole, userRole, StringComparison.OrdinalIgnoreCase)) return true;

            // Warehouse Manager aliases
            if ((recipientRole.Equals("WarehouseManager", StringComparison.OrdinalIgnoreCase) || recipientRole.Equals("WManager", StringComparison.OrdinalIgnoreCase))
                && (userRole.Equals("WarehouseManager", StringComparison.OrdinalIgnoreCase) || userRole.Equals("WManager", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Branch Manager aliases
            if ((recipientRole.Equals("BranchManager", StringComparison.OrdinalIgnoreCase) || recipientRole.Equals("BManager", StringComparison.OrdinalIgnoreCase))
                && (userRole.Equals("BranchManager", StringComparison.OrdinalIgnoreCase) || userRole.Equals("BManager", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Regional Manager aliases
            if ((recipientRole.Equals("RegionalManager", StringComparison.OrdinalIgnoreCase) || recipientRole.Equals("RManager", StringComparison.OrdinalIgnoreCase))
                && (userRole.Equals("RegionalManager", StringComparison.OrdinalIgnoreCase) || userRole.Equals("RManager", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Bartender / Barista aliases
            if ((recipientRole.Equals("Bartender", StringComparison.OrdinalIgnoreCase) || recipientRole.Equals("Barista", StringComparison.OrdinalIgnoreCase))
                && (userRole.Equals("Bartender", StringComparison.OrdinalIgnoreCase) || userRole.Equals("Barista", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        public static bool IsTargetedForUser(Notification n, int userId, string userRole, string? branchId)
        {
            if (n.RecipientUserId.HasValue)
            {
                return n.RecipientUserId.Value == userId;
            }

            // Strict role matching
            if (!IsRoleMatch(n.RecipientRole, userRole))
            {
                return false;
            }

            // Central roles receive targeted notifications across all branches
            bool isCentralRole = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                              || userRole.Equals("RManager", StringComparison.OrdinalIgnoreCase)
                              || userRole.Equals("RegionalManager", StringComparison.OrdinalIgnoreCase)
                              || userRole.Equals("WarehouseManager", StringComparison.OrdinalIgnoreCase)
                              || userRole.Equals("WManager", StringComparison.OrdinalIgnoreCase);

            bool branchMatches = string.IsNullOrEmpty(n.BranchId)
                || n.BranchId == branchId
                || isCentralRole;

            return branchMatches;
        }

        /// <inheritdoc/>
        public async Task<bool> MarkReadAsync(int notificationId, int userId, string userRole, string? branchId = null)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null || !IsTargetedForUser(notification, userId, userRole, branchId))
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task ClearAllAsync(int userId, string userRole, string? branchId = null)
        {
            var notifications = await _context.Notifications
                .Where(n => !n.IsRead)
                .ToListAsync();

            var userNotifications = notifications.Where(n => IsTargetedForUser(n, userId, userRole, branchId));

            foreach (var n in userNotifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Notification>> GetAllForUserAsync(int userId, string userRole, string? branchId = null)
        {
            var allNotifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedTime)
                .ToListAsync();

            return allNotifications.Where(n => IsTargetedForUser(n, userId, userRole, branchId)).ToList();
        }
    }
}
