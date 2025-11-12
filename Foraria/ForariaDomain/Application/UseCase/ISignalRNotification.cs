using ForariaDomain.Models;

namespace ForariaDomain.Application.UseCase;

public interface ISignalRNotification
{
    Task NotifyPollUpdatedAsync(int pollId, IEnumerable<PollResult> results);
}
