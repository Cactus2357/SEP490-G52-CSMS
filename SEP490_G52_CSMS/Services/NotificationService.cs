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
                ResourceUrl = evt.ResourceUrl
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> MarkReadAsync(int notificationId, int userId, string userRole)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId
                    && (n.RecipientUserId == userId || (n.RecipientUserId == null && n.RecipientRole == userRole) || (n.RecipientUserId == null && n.RecipientRole == null)));

            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task ClearAllAsync(int userId, string userRole)
        {
            var notifications = await _context.Notifications
                .Where(n => !n.IsRead
                    && (n.RecipientUserId == userId || (n.RecipientUserId == null && n.RecipientRole == userRole) || (n.RecipientUserId == null && n.RecipientRole == null)))
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Notification>> GetAllForUserAsync(int userId, string userRole)
        {
            return await _context.Notifications
                .Where(n => n.RecipientUserId == userId || (n.RecipientUserId == null && n.RecipientRole == userRole) || (n.RecipientUserId == null && n.RecipientRole == null))
                .OrderByDescending(n => n.CreatedTime)
                .ToListAsync();
        }
    }
}
