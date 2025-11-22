using Foraria.Application.UseCase;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.SignalR;

public class CallHub : Hub
{
    private readonly ToggleMute _toggleMute;
    private readonly ToggleCamera _toggleCamera;
    private readonly LeaveCall _leaveCall;
    private readonly SaveCallMessage _saveCallMessage;

    private static readonly Dictionary<string, (string CallId, int UserId)> _connections = new();

    public CallHub(
        ToggleMute toggleMute,
        ToggleCamera toggleCamera,
        LeaveCall leaveCall,
        SaveCallMessage saveCallMessage)
    {
        _toggleMute = toggleMute;
        _toggleCamera = toggleCamera;
        _leaveCall = leaveCall;
        _saveCallMessage = saveCallMessage;
    }



    public async Task JoinCall(string callId, int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, callId);

        _connections[Context.ConnectionId] = (callId, userId);

        var usersInCall = _connections
            .Where(c => c.Value.CallId == callId)
            .Select(c => new
            {
                connectionId = c.Key,
                userId = c.Value.UserId
            })
            .ToList();

        await Clients.Caller.SendAsync("CurrentParticipants", usersInCall);

        await Clients.OthersInGroup(callId)
            .SendAsync("UserJoined", new
            {
                userId,
                connectionId = Context.ConnectionId
            });
    }



    public async Task LeaveCall(string callId, int userId)
    {
        _leaveCall.Execute(int.Parse(callId), userId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, callId);
        _connections.Remove(Context.ConnectionId);

        await Clients.Group(callId)
            .SendAsync("UserLeft", new { userId, connectionId = Context.ConnectionId });
    }


    public async Task SendOffer(string callId, int toUserId, object offer)
    {
        var targetConnectionId = _connections
            .FirstOrDefault(x => x.Value.CallId == callId && x.Value.UserId == toUserId)
            .Key;

        if (targetConnectionId != null)
        {
            var fromUserId = _connections.TryGetValue(Context.ConnectionId, out var info) ? info.UserId : 0;

            await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", new
            {
                from = Context.ConnectionId,
                fromUserId,
                offer
            });
        }
    }

    public async Task SendAnswer(string callId, int toUserId, object answer)
    {
        var targetConnectionId = _connections
            .FirstOrDefault(x => x.Value.CallId == callId && x.Value.UserId == toUserId)
            .Key;

        if (targetConnectionId != null)
        {
            var fromUserId = _connections.TryGetValue(Context.ConnectionId, out var info) ? info.UserId : 0;

            await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", new
            {
                from = Context.ConnectionId,
                fromUserId,
                answer
            });
        }
    }


    public async Task SendIceCandidate(string callId, int toUserId, object candidate)
    {
        var targetConnectionId = _connections
            .FirstOrDefault(x => x.Value.CallId == callId && x.Value.UserId == toUserId)
            .Key;

        if (targetConnectionId != null)
        {
            var fromUserId = _connections.TryGetValue(Context.ConnectionId, out var info) ? info.UserId : 0;

            await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", new
            {
                from = Context.ConnectionId,
                fromUserId,
                candidate
            });
        }
    }


    public async Task ToggleMute(string callId, int userId, bool isMuted)
    {
        _toggleMute.Execute(int.Parse(callId), userId, isMuted);

        await Clients.Group(callId).SendAsync("UserMuteChanged", new
        {
            userId,
            isMuted
        });
    }

    public async Task ToggleCamera(string callId, int userId, bool isCameraOn)
    {
        _toggleCamera.Execute(int.Parse(callId), userId, isCameraOn);

        await Clients.Group(callId).SendAsync("UserCameraChanged", new
        {
            userId,
            isCameraOn
        });
    }


    public async Task SendChatMessage(string callId, int userId, string message)
    {
        _saveCallMessage.Execute(int.Parse(callId), userId, message);

        await Clients.Group(callId).SendAsync("ReceiveChatMessage", new
        {
            userId,
            message,
            sentAt = DateTime.Now
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out var info))
        {
            var (callId, userId) = info;

            _leaveCall.Execute(int.Parse(callId), userId);

            await Clients.Group(callId).SendAsync("UserLeft", new
            {
                userId,
                connectionId = Context.ConnectionId
            });

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, callId);
            _connections.Remove(Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
