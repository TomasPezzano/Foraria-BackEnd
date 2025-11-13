using Microsoft.AspNetCore.SignalR;

namespace Foraria.Hubs
{
    public class CallHub : Hub
    {
        public async Task JoinCall(string callId, int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, callId);

            await Clients.Group(callId).SendAsync("UserJoined", new
            {
                userId,
                connectionId = Context.ConnectionId
            });
        }

        public async Task SendOffer(string callId, int toUserId, object offer)
        {
            await Clients.Group(callId).SendAsync("ReceiveOffer", new
            {
                from = Context.ConnectionId,
                toUserId,
                offer
            });
        }

        public async Task SendAnswer(string callId, int toUserId, object answer)
        {
            await Clients.Group(callId).SendAsync("ReceiveAnswer", new
            {
                from = Context.ConnectionId,
                toUserId,
                answer
            });
        }

        public async Task SendIceCandidate(string callId, int toUserId, object candidate)
        {
            await Clients.Group(callId).SendAsync("ReceiveIceCandidate", new
            {
                from = Context.ConnectionId,
                toUserId,
                candidate
            });
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task LeaveCall(string callId, int userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, callId);

            await Clients.Group(callId)
                .SendAsync("UserLeft", new { userId, connectionId = Context.ConnectionId });
        }
    }
}
