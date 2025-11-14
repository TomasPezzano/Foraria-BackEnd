using Foraria.Domain.Repository;
using Foraria.Infrastructure.Notifications;
using ForariaDomain;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase;

public interface ISendVotingNotification
{
    Task NotifyNewVotingAsync(int pollId);
    Task NotifyVotingClosingSoonAsync(int pollId);
}

public class SendVotingNotification : ISendVotingNotification
{
    private readonly IPollRepository _pollRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationDispatcher _notificationDispatcher;

    public SendVotingNotification(
        IPollRepository pollRepository,
        IUserRepository userRepository,
        INotificationDispatcher notificationDispatcher)
    {
        _pollRepository = pollRepository;
        _userRepository = userRepository;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task NotifyNewVotingAsync(int pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll == null)
        {
            throw new KeyNotFoundException($"No se encontró la votación con ID {pollId}");
        }

        // Obtener todos los usuarios con derecho a voto
        var users = await GetUsersWithVotingRightsAsync();

        if (!users.Any())
        {
            return;
        }

        var title = "🗳️ Nueva Votación Disponible";
        var body = $"{poll.Title} - Votá hasta el {poll.EndDate:dd/MM/yyyy}";

        var metadata = new Dictionary<string, string>
        {
            { "pollId", poll.Id.ToString() },
            { "title", poll.Title },
            { "endDate", poll.EndDate.ToString("yyyy-MM-dd") },
            { "categoryId", poll.CategoryPoll_id.ToString() }
        };

        var userIds = users.Select(u => u.Id).ToList();

        await _notificationDispatcher.SendBatchNotificationAsync(
            userIds: userIds,
            type: NotificationType.VotingAvailable,
            title: title,
            body: body,
            relatedEntityId: poll.Id,
            relatedEntityType: "Poll",
            metadata: metadata
        );
    }

    public async Task NotifyVotingClosingSoonAsync(int pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll == null)
        {
            throw new KeyNotFoundException($"No se encontró la votación con ID {pollId}");
        }

        var hoursUntilClose = (poll.EndDate - DateTime.UtcNow).TotalHours;
        if (hoursUntilClose < 0 || hoursUntilClose > 24)
        {
            return;
        }

        var usersWithRights = await GetUsersWithVotingRightsAsync();
        var usersWhoVoted = poll.Votes.Select(v => v.User_id).ToHashSet();
        var usersToNotify = usersWithRights
            .Where(u => !usersWhoVoted.Contains(u.Id))
            .ToList();

        if (!usersToNotify.Any())
        {
            return;
        }

        var title = "⏰ Votación por Cerrar";
        var body = $"¡Última oportunidad! La votación '{poll.Title}' cierra hoy. ¡No te olvides de votar!";

        var metadata = new Dictionary<string, string>
        {
            { "pollId", poll.Id.ToString() },
            { "title", poll.Title },
            { "endDate", poll.EndDate.ToString("yyyy-MM-dd HH:mm") }
        };

        var userIds = usersToNotify.Select(u => u.Id).ToList();

        await _notificationDispatcher.SendBatchNotificationAsync(
            userIds: userIds,
            type: NotificationType.VotingClosingSoon,
            title: title,
            body: body,
            relatedEntityId: poll.Id,
            relatedEntityType: "Poll",
            metadata: metadata
        );
    }

    private async Task<List<User>> GetUsersWithVotingRightsAsync()
    {
        var allUsers = await _userRepository.GetAllAsync();

        return allUsers.Where(u =>
            u.Role.Description == "Consorcio" ||
            u.Role.Description == "Administrador" ||
            u.Role.Description == "Propietario" ||
            (u.Role.Description == "Inquilino" && u.HasPermission)
        ).ToList();
    }
}