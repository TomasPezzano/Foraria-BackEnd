using ForariaDomain;
using ForariaDomain.Repository;

namespace Foraria.Infrastructure.Persistence;

public class CallParticipantRepository : ICallParticipantRepository
{
    private readonly ForariaContext _context;

    public CallParticipantRepository(ForariaContext context)
    {
        _context = context;
    }

    public void Add(CallParticipant participant)
    {
        _context.CallParticipants.Add(participant);
        _context.SaveChanges();
    }

    public List<CallParticipant> GetByCallId(int callId)
    {
        return _context.CallParticipants
            .Where(p => p.CallId == callId)
            .ToList();
    }

    public void Update(CallParticipant participant)
    {
        _context.CallParticipants.Update(participant);
        _context.SaveChanges();
    }
}
