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

        public async Task<Poll?> GetById(int id)
        {
            return await _context.Polls
                .Include(p => p.User)
                .Include(p => p.ResultPoll)
                .Include(p => p.PollOptions)
                .Include(p => p.Votes)
                .Include(p => p.BlockchainProof)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<IEnumerable<Poll>> GetActivePolls(int consortiumId, DateTime now)
        {
            return await _context.Polls
                .Include(p => p.User)
                    .ThenInclude(u => u.Residences)
                        .ThenInclude(r => r.Consortium)
                .Where(p =>
                    p.State == "Active" &&
                    p.StartDate <= now &&
                    p.EndDate >= now &&
                    p.User.Residences.Any(r => r.Consortium.Id == consortiumId))
                .ToListAsync();
        }
    }
}
