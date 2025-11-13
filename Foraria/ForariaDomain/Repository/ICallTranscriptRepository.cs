namespace ForariaDomain.Repository;

public interface ICallTranscriptRepository
{
    CallTranscript Create(CallTranscript transcript);
    CallTranscript? GetByCallId(int callId);
    CallTranscript? GetById(int id);
}
