using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence
{
    public class CallMessageRepository : ICallMessageRepository
    {
        private readonly ForariaContext _context;

        public CallMessageRepository(ForariaContext context)
        {
            _context = context;
        }

        public void Save(CallMessage message)
        {
            _context.CallMessages.Add(message);
            _context.SaveChanges();
        }

        public List<CallMessage> GetLastByCall(int callId, int limit = 50)
        {
            return _context.CallMessages
                .Where(m => m.CallId == callId)
                .OrderByDescending(m => m.SentAt)
                .Take(limit)
                .AsNoTracking()
                .ToList();
        }
    }
}
