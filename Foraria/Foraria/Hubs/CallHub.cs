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

        await Clients.Group(callId).SendAsync("UserJoined", new
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
