using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface ISendPollNotification
{
    Task ExecuteForNewPollAsync(int pollId);
    Task ExecuteForClosingSoonAsync(int pollId);
    Task ExecuteForClosedAsync(int pollId);
}

public class SendPollNotification : ISendPollNotification
{
    private readonly IPollRepository _pollRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationDispatcher _notificationDispatcher;

    public SendPollNotification(
        IPollRepository pollRepository,
        IVoteRepository voteRepository,
        IUserRepository userRepository,
        INotificationDispatcher notificationDispatcher)
    {
        _pollRepository = pollRepository;
        _voteRepository = voteRepository;
        _userRepository = userRepository;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task ExecuteForNewPollAsync(int pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll == null)
        {
            throw new KeyNotFoundException($"No se encontró la votación con ID {pollId}");
        }

        var users = await _userRepository.GetUsersByConsortiumIdAsync();

        var usersToNotify = users.Where(u =>
            u.Role.Description == "Propietario" ||
            u.Role.Description == "Consorcio" || u.Role.Description == "Administrador"
        ).ToList();

        if (!usersToNotify.Any())
        {
            return;
        }

        var title = "🗳️ Nueva Votación Disponible";
        var body = $"Nueva votación: '{poll.Title}'. Tienes hasta el {poll.EndDate:dd/MM/yyyy} para votar.";

        var metadata = new Dictionary<string, string>
        {
            { "pollId", poll.Id.ToString() },
            { "title", poll.Title },
            { "endDate", poll.EndDate.ToString("yyyy-MM-dd") },
            { "category", poll.CategoryPoll.Description }
        };

        var userIds = usersToNotify.Select(u => u.Id).ToList();

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

    public async Task ExecuteForClosingSoonAsync(int pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll == null)
        {
            throw new KeyNotFoundException($"No se encontró la votación con ID {pollId}");
        }

        var users = await _userRepository.GetUsersByConsortiumIdAsync();
        var votes = await _voteRepository.GetVotesByPollIdAsync(pollId);

        var usersWhoVoted = votes.Select(v => v.User_id).ToHashSet();

        var usersToNotify = users.Where(u =>
            (u.Role.Description == "Propietario" || u.Role.Description == "Consorcio" || u.Role.Description == "Administrador") &&
            !usersWhoVoted.Contains(u.Id)
        ).ToList();

        if (!usersToNotify.Any())
        {
            return;
        }

        var hoursRemaining = (poll.EndDate - DateTime.Now).TotalHours;
        var timeLeft = hoursRemaining <= 24
            ? $"{(int)hoursRemaining} horas"
            : $"{(int)(hoursRemaining / 24)} días";

        var title = "⏰ Votación por Cerrar";
        var body = $"La votación '{poll.Title}' cierra en {timeLeft}. ¡No olvides votar!";

        var metadata = new Dictionary<string, string>
        {
            { "pollId", poll.Id.ToString() },
            { "title", poll.Title },
            { "endDate", poll.EndDate.ToString("yyyy-MM-dd") }
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

    public async Task ExecuteForClosedAsync(int pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll == null)
        {
            throw new KeyNotFoundException($"No se encontró la votación con ID {pollId}");
        }

        var users = await _userRepository.GetUsersByConsortiumIdAsync();

        var usersToNotify = users.Where(u =>
            u.Role.Description == "Propietario" ||
            u.Role.Description == "Consorcio" || u.Role.Description == "Administrador"
        ).ToList();

        if (!usersToNotify.Any())
        {
            return;
        }

        var title = "✅ Votación Cerrada";
        var body = $"La votación '{poll.Title}' ha finalizado. Consulta los resultados.";

        var metadata = new Dictionary<string, string>
        {
            { "pollId", poll.Id.ToString() },
            { "title", poll.Title }
        };

        var userIds = usersToNotify.Select(u => u.Id).ToList();

        await _notificationDispatcher.SendBatchNotificationAsync(
            userIds: userIds,
            type: NotificationType.VotingClosed,
            title: title,
            body: body,
            relatedEntityId: poll.Id,
            relatedEntityType: "Poll",
            metadata: metadata
        );
    }
}