using Foraria.Hubs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Models;
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

        public async Task NotifyPollUpdatedAsync(int pollId, IEnumerable<PollResult> results)
        {
            await _hubContext.Clients.All.SendAsync("PollUpdated", new { pollId, results });
        }
    }
}
