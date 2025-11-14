using ForariaDomain;

public interface INotificationDispatcher
{
    Task<Notification> SendNotificationAsync(
        int userId,
        string type,
        string title,
        string body,
        int? relatedEntityId = null,
        string? relatedEntityType = null,
        Dictionary<string, string>? metadata = null);

    Task<List<Notification>> SendBatchNotificationAsync(
        List<int> userIds,
        string type,
        string title,
        string body,
        int? relatedEntityId = null,
        string? relatedEntityType = null,
        Dictionary<string, string>? metadata = null);

    Task ProcessPendingNotificationsAsync();
}