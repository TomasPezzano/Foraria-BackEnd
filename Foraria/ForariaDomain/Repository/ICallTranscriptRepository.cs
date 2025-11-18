namespace ForariaDomain.Repository;

public interface ICallTranscriptRepository
{
    CallTranscript Create(CallTranscript transcript);
    CallTranscript? GetByCallId(int callId);
    CallTranscript? GetById(int id);
    void Update(CallTranscript transcript);
    IEnumerable<CallTranscript> GetAll();
    CallTranscript? GetByCallIdTranscript(int callId);

}
