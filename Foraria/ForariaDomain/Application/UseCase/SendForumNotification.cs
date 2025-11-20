using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface ISendForumNotification
{
    Task ExecuteForNewThreadAsync(int threadId);
    Task ExecuteForNewMessageAsync(int messageId);
}

public class SendForumNotification : ISendForumNotification
{
    private readonly IThreadRepository _threadRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationDispatcher _notificationDispatcher;

    public SendForumNotification(
        IThreadRepository threadRepository,
        IUserRepository userRepository,
        INotificationDispatcher notificationDispatcher)
    {
        _threadRepository = threadRepository;
        _userRepository = userRepository;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task ExecuteForNewThreadAsync(int threadId)
    {
        var thread = await _threadRepository.GetById(threadId);
        if (thread == null)
        {
            throw new KeyNotFoundException($"No se encontró el hilo con ID {threadId}");
        }

        var users = await _userRepository.GetUsersByConsortiumIdAsync(thread.ConsortiumId);

        // Notificar a todos excepto al creador del thread
        var usersToNotify = users
            .Where(u => u.Id != thread.UserId)
            .ToList();

        if (!usersToNotify.Any())
        {
            return;
        }

        var title = "💬 Nuevo Tema en el Foro";
        var body = $"Nuevo tema: '{thread.Theme}' en {thread.Forum.Category}";

        var metadata = new Dictionary<string, string>
        {
            { "threadId", thread.Id.ToString() },
            { "theme", thread.Theme },
            { "forumId", thread.ForumId.ToString() },
            { "category", thread.Forum.Category.ToString() },
            { "createdBy", thread.UserId.ToString() }
        };

        var userIds = usersToNotify.Select(u => u.Id).ToList();

        await _notificationDispatcher.SendBatchNotificationAsync(
            userIds: userIds,
            type: NotificationType.ForumNewThread,
            title: title,
            body: body,
            relatedEntityId: thread.Id,
            relatedEntityType: "Thread",
            metadata: metadata
        );
    }

    public async Task ExecuteForNewMessageAsync(int messageId)
    {
        var thread = await _threadRepository.GetByIdWithMessagesAsync(messageId);
        if (thread == null)
        {
            throw new KeyNotFoundException($"No se encontró el mensaje con ID {messageId}");
        }

        var message = thread.Messages.FirstOrDefault(m => m.Id == messageId);
        if (message == null)
        {
            throw new KeyNotFoundException($"No se encontró el mensaje con ID {messageId}");
        }

        var participantIds = thread.Messages
            .Select(m => m.User_id)
            .Append(thread.UserId) 
            .Distinct()
            .Where(userId => userId != message.User_id) 
            .ToList();

        if (!participantIds.Any())
        {
            return;
        }

        var title = "💬 Nueva Respuesta en el Foro";
        var body = $"Hay una nueva respuesta en '{thread.Theme}'";

        var metadata = new Dictionary<string, string>
        {
            { "messageId", message.Id.ToString() },
            { "threadId", thread.Id.ToString() },
            { "theme", thread.Theme },
            { "respondedBy", message.User_id.ToString() }
        };

        await _notificationDispatcher.SendBatchNotificationAsync(
            userIds: participantIds,
            type: NotificationType.ForumActivity,
            title: title,
            body: body,
            relatedEntityId: thread.Id,
            relatedEntityType: "Thread",
            metadata: metadata
        );
    }
}
