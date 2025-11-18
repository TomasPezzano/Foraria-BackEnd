using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase;

public interface IConfigureNotificationPreferences
{
    Task<NotificationPreference> UpdatePreferencesAsync(int userId, NotificationPreference preferences);
    Task UpdateFcmTokenAsync(int userId, string fcmToken);
    Task<NotificationPreference> GetPreferencesAsync(int userId);
}

public class ConfigureNotificationPreferences : IConfigureNotificationPreferences
{
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IUserRepository _userRepository;

    public ConfigureNotificationPreferences(
        INotificationPreferenceRepository preferenceRepository,
        IUserRepository userRepository)
    {
        _preferenceRepository = preferenceRepository;
        _userRepository = userRepository;
    }

    public async Task<NotificationPreference> UpdatePreferencesAsync(
        int userId,
        NotificationPreference preferences)
    {
        // ✅ FIX: Usar método async
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"No se encontró el usuario con ID {userId}");
        }

        preferences.UserId = userId;
        preferences.IsConfigured = true;
        preferences.UpdatedAt = DateTime.Now;

        return await _preferenceRepository.UpsertAsync(preferences);
    }

    public async Task UpdateFcmTokenAsync(int userId, string fcmToken)
    {
        // ✅ FIX: Usar método async
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"No se encontró el usuario con ID {userId}");
        }

        if (string.IsNullOrWhiteSpace(fcmToken))
        {
            throw new ArgumentException("El FCM token no puede estar vacío", nameof(fcmToken));
        }

        await _preferenceRepository.UpdateFcmTokenAsync(userId, fcmToken);
    }

    public async Task<NotificationPreference> GetPreferencesAsync(int userId)
    {
        var preferences = await _preferenceRepository.GetByUserIdAsync(userId);

        if (preferences == null)
        {
            // Retornar preferencias por defecto sin crear en BD
            return new NotificationPreference
            {
                UserId = userId,
                PushEnabled = true,
                EmailEnabled = false,
                SmsEnabled = false,
                ExpenseNotificationsEnabled = true,
                MeetingNotificationsEnabled = true,
                VotingNotificationsEnabled = false,
                ForumNotificationsEnabled = true,
                MaintenanceNotificationsEnabled = true,
                ClaimNotificationsEnabled = true,
                IsConfigured = false
            };
        }

        return preferences;
    }
}