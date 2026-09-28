namespace SEP490_G52_CSMS.Services
{
    /// <summary>
    /// Immutable payload for raising a notification event through INotificationService.
    /// </summary>
    public record NotificationEvent(
        string Title,
        string Message,
        int? RecipientUserId = null,
        string? RecipientRole = null,
        string? ResourceUrl = null,
        string? BranchId = null
    );
}
