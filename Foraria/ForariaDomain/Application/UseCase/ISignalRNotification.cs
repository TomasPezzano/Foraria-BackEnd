namespace ForariaDomain.Application.UseCase
{

    public interface ISignalRNotification
    {
        Task NotifyPollUpdatedAsync(int pollId, IEnumerable<PollResultDto> results);
    }

}
