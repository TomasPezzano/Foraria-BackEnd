using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Repository;

public class CallTranscriptRepository : ICallTranscriptRepository
{
    private readonly ForariaContext _context;

    public CallTranscriptRepository(ForariaContext context)
    {
        _context = context;
    }

    public CallTranscript Create(CallTranscript transcript)
    {
        _context.CallTranscripts.Add(transcript);
        return transcript;
    }

    public CallTranscript? GetByCallId(int callId)
    {
        return _context.CallTranscripts
            .FirstOrDefault(t => t.CallId == callId);
    }

    public CallTranscript? GetById(int id)
    {
        return _context.CallTranscripts
            .FirstOrDefault(t => t.Id == id);
    }
}
