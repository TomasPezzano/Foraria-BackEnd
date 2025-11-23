using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Infrastructure.Persistence;

public class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly ForariaContext _context;

    public NotificationPreferenceRepository(ForariaContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreference?> GetByUserIdAsync(int userId)
    {
        return await _context.NotificationPreferences
            .Include(np => np.User)
            .FirstOrDefaultAsync(np => np.UserId == userId);
    }

    public async Task<NotificationPreference> UpsertAsync(NotificationPreference preference)
    {
        var existing = await GetByUserIdAsync(preference.UserId);

        if (existing != null)
        {
            existing.PushEnabled = preference.PushEnabled;
            existing.EmailEnabled = preference.EmailEnabled;
            existing.SmsEnabled = preference.SmsEnabled;
            existing.ExpenseNotificationsEnabled = preference.ExpenseNotificationsEnabled;
            existing.MeetingNotificationsEnabled = preference.MeetingNotificationsEnabled;
            existing.VotingNotificationsEnabled = preference.VotingNotificationsEnabled;
            existing.ForumNotificationsEnabled = preference.ForumNotificationsEnabled;
            existing.MaintenanceNotificationsEnabled = preference.MaintenanceNotificationsEnabled;
            existing.ClaimNotificationsEnabled = preference.ClaimNotificationsEnabled;
            existing.FcmToken = preference.FcmToken;
            existing.IsConfigured = preference.IsConfigured;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            preference.UpdatedAt = DateTime.Now;
            _context.NotificationPreferences.Add(preference);
            await _context.SaveChangesAsync();
            return preference;
        }
    }

    public async Task UpdateFcmTokenAsync(int userId, string fcmToken)
    {
        var preference = await GetByUserIdAsync(userId);

        if (preference != null)
        {
            preference.FcmToken = fcmToken;
            preference.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        else
        {
            var newPreference = new NotificationPreference
            {
                UserId = userId,
                FcmToken = fcmToken,
                PushEnabled = true,
                UpdatedAt = DateTime.Now
            };
            _context.NotificationPreferences.Add(newPreference);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<NotificationPreference>> GetUsersWithNotificationTypeEnabledAsync(
        string notificationType)
    {
        return notificationType switch
        {
            "Expense" => await _context.NotificationPreferences
                .Include(np => np.User)
                .Where(np => np.ExpenseNotificationsEnabled)
                .ToListAsync(),

            "Meeting" => await _context.NotificationPreferences
                .Include(np => np.User)
                .Where(np => np.MeetingNotificationsEnabled)
                .ToListAsync(),

            "Voting" => await _context.NotificationPreferences
                .Include(np => np.User)
                .Where(np => np.VotingNotificationsEnabled)
                .ToListAsync(),

            "Forum" => await _context.NotificationPreferences
                .Include(np => np.User)
                .Where(np => np.ForumNotificationsEnabled)
                .ToListAsync(),

            "Maintenance" => await _context.NotificationPreferences
                .Include(np => np.User)
                .Where(np => np.MaintenanceNotificationsEnabled)
                .ToListAsync(),

            "Claim" => await _context.NotificationPreferences
                .Include(np => np.User)
                .Where(np => np.ClaimNotificationsEnabled)
                .ToListAsync(),

            _ => new List<NotificationPreference>()
        };
    }

    public async Task<IEnumerable<NotificationPreference>> GetUsersWithPushEnabledAsync()
    {
        return await _context.NotificationPreferences
            .Include(np => np.User)
            .Where(np => np.PushEnabled && !string.IsNullOrEmpty(np.FcmToken))
            .ToListAsync();
    }
}
