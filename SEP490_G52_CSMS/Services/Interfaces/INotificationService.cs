using SEP490_G52_CSMS.Models;

namespace SEP490_G52_CSMS.Services.Interfaces
{
    public interface INotificationService
    {
        /// <summary>Persists a notification to the DB for the given recipients.</summary>
        Task SendAsync(NotificationEvent evt);

        /// <summary>Marks a single notification as read for the given user.</summary>
        Task<bool> MarkReadAsync(int notificationId, int userId, string userRole, string? branchId = null);

        /// <summary>Marks all notifications as read for the given user / role (clear all).</summary>
        Task ClearAllAsync(int userId, string userRole, string? branchId = null);

        /// <summary>Returns all notifications (read + unread) for the user, newest first.</summary>
        Task<List<Notification>> GetAllForUserAsync(int userId, string userRole, string? branchId = null);
    }
}
