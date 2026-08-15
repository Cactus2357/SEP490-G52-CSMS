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
                CreatedTime = DateTime.Now,
                IsRead = false,
                RecipientUserId = evt.RecipientUserId,
                RecipientRole = evt.RecipientRole,
                ResourceUrl = evt.ResourceUrl,
                BranchId = evt.BranchId
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        private static bool IsTargetedForUser(Notification n, int userId, string userRole, string? branchId)
        {
            if (n.RecipientUserId.HasValue)
            {
                return n.RecipientUserId.Value == userId;
            }

            bool isCentralRole = userRole == "RManager" || userRole == "WarehouseManager" || userRole == "WManager";

            bool roleMatches = string.IsNullOrEmpty(n.RecipientRole)
                || n.RecipientRole == userRole
                || (n.RecipientRole == "WarehouseManager" && (userRole == "WManager" || userRole == "WarehouseManager"))
                || (n.RecipientRole == "WManager" && (userRole == "WManager" || userRole == "WarehouseManager"))
                || isCentralRole;

            bool branchMatches = string.IsNullOrEmpty(n.BranchId)
                || n.BranchId == branchId
                || isCentralRole;

            return roleMatches && branchMatches;
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
