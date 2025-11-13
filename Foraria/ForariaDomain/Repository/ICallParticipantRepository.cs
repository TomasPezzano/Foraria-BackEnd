namespace ForariaDomain.Repository;

public interface ICallParticipantRepository
{
    void Add(CallParticipant participant);
    List<CallParticipant> GetByCallId(int callId);
    void Update(CallParticipant participant);
}
