namespace ForariaDomain.Repository;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);

    Task<Notification?> GetByIdAsync(int id);

    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);

    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId);

    Task<IEnumerable<Notification>> GetPendingNotificationsAsync();

    Task MarkAsSentAsync(int notificationId);

    Task MarkAsReadAsync(int notificationId);

    Task MarkAsFailedAsync(int notificationId, string errorMessage);

    Task UpdateAsync(Notification notification);

    Task DeleteOlderThanAsync(DateTime date);
}
