using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Models;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence
{
    public class VoteRepositoryImplementation : IVoteRepository
    {

        private readonly ForariaContext _context;

        public VoteRepositoryImplementation(ForariaContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Vote vote)
        {
            await _context.Votes.AddAsync(vote);
        }

        public async Task<Vote?> GetByUserAndPollAsync(int userId, int pollId)
        {
            return await _context.Votes
                .FirstOrDefaultAsync(v => v.User_id == userId && v.Poll_id == pollId);
        }

        public async Task<IEnumerable<PollResult>> GetPollResultsAsync(int pollId)
        {
            return await _context.Votes
                .Where(v => v.Poll_id == pollId)
                .GroupBy(v => v.PollOption_id)
                .Select(g => new PollResult
                {
                    PollOptionId = g.Key,
                    VotesCount = g.Count()
                })
                .ToListAsync();
        }

    }
}
