namespace ForariaDomain.Repository;

public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetByUserIdAsync(int userId);
    Task<NotificationPreference> UpsertAsync(NotificationPreference preference);
    Task UpdateFcmTokenAsync(int userId, string fcmToken);
    Task<IEnumerable<NotificationPreference>> GetUsersWithNotificationTypeEnabledAsync(
        string notificationType);
    Task<IEnumerable<NotificationPreference>> GetUsersWithPushEnabledAsync();
}
