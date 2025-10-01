using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence
{
    public class PollRepositoryImplementation : IPollRepository
    {

        private readonly ForariaContext _context;

        public PollRepositoryImplementation(ForariaContext context)
        {
            _context = context;
        }

        public async Task CreatePoll(Poll poll)
        {
            await _context.Set<Poll>().AddAsync(poll);

        }

        public async Task<List<Poll>> GetAllPolls()
        {
            return await _context.Polls
                         .Include(p => p.PollOptions) 
                         .ToListAsync();
        }
    }
}
