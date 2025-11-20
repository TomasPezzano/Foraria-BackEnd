namespace ForariaDomain.Repository
{
    public interface ICallParticipantRepository
    {
        void Add(CallParticipant participant);
        List<CallParticipant> GetByCallId(int callId);
        IEnumerable<CallParticipant> GetParticipants(int callId);
        void Update(CallParticipant participant);
        void SetMute(int callId, int userId, bool isMuted);
        void SetCamera(int callId, int userId, bool isCameraOn);
        void SetDisconnected(int callId, int userId);
        bool IsUserInCall(int callId, int userId);
        int CountByCallId(int callId);
    }
}
