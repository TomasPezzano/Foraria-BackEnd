using Foraria.Hubs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.SignalR;

namespace Foraria.SignalRImplementation
{
    public class SignalRNotification : ISignalRNotification
    {
        private readonly IHubContext<PollHub> _hubContext;

        public SignalRNotification(IHubContext<PollHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyPollUpdatedAsync(int pollId, IEnumerable<ResultPoll> results)
        {
            await _hubContext.Clients.All.SendAsync("PollUpdated", new { pollId, results });
        }
    }
}
