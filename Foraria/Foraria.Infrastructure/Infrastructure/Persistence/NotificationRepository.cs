using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Infrastructure.Persistence;

public class NotificationRepository : INotificationRepository
{
    private readonly ForariaContext _context;
    public NotificationRepository(ForariaContext context)
    {
        _context = context;
    }
    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }
    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id);
    }
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && n.ReadAt == null)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
    public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync()
    {
        return await _context.Notifications
            .Include(n => n.User)
            .Where(n => n.Status == NotificationStatus.Pending)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();
    }
    public async Task MarkAsSentAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Read;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    public async Task MarkAsFailedAsync(int notificationId, string errorMessage)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = errorMessage;
            await _context.SaveChangesAsync();
        }
    }
    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteOlderThanAsync(DateTime date)
    {
        var oldNotifications = await _context.Notifications
            .Where(n => n.CreatedAt < date)
            .ToListAsync();
        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();
    }
}
