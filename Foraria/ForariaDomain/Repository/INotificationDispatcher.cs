using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository;

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
