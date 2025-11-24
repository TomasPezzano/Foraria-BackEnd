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



        public async Task<Poll?> GetPollWithResultsAsync(int pollId)
        {
            return await _context.Polls
                .Where(p => p.Id == pollId)
                .Include(p => p.PollOptions)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Poll>> GetAllPollsWithResultsAsync()
        {
            return await _context.Polls
                .Include(p => p.PollOptions)
                .Include(p => p.Votes)
                .ToListAsync();
        }

        public async Task UpdatePoll(Poll poll)
        {
            var existingPoll = await _context.Polls.FindAsync(poll.Id);
            if (existingPoll == null)
                return;

            _context.Entry(existingPoll).CurrentValues.SetValues(poll);

        }


        public async Task<IEnumerable<Poll>> GetActivePolls(int consortiumId, DateTime now)
        {
            return await _context.Polls
                .Include(p => p.User)
                    .ThenInclude(u => u.Residences)
                        .ThenInclude(r => r.Consortium)
                .Where(p =>
                    p.State == "Activa" &&
                    p.StartDate <= now &&
                    p.EndDate >= now)
                .ToListAsync();
        }

        public async Task<Poll> AddAsync(Poll poll)
        {
            await _context.Polls.AddAsync(poll);
            await _context.SaveChangesAsync();
            return poll;
        }

        public async Task<Poll?> GetByIdAsync(int id)
        {
            return await _context.Polls
                .Include(p => p.User)
                .Include(p => p.PollOptions)
                .Include(p => p.Votes)
                    .ThenInclude(v => v.User)
                .Include(p => p.CategoryPoll)
                .Include(p => p.ResultPoll)
                .Include(p => p.ApprovedByUser)
                .Include(p => p.BlockchainProof)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Poll>> GetByConsortiumIdAsync()
        {
            return await _context.Polls
                .Include(p => p.User)
                .Include(p => p.PollOptions)
                .Include(p => p.Votes)
                .Include(p => p.CategoryPoll)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Poll>> GetActiveByConsortiumIdAsync()
        {
            var now = DateTime.Now;
            return await _context.Polls
                .Include(p => p.User)
                .Include(p => p.PollOptions)
                .Where(p => p.State == "Active"
                    && p.StartDate <= now
                    && p.EndDate >= now)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Poll>> GetClosingSoonAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Polls
                .Include(p => p.User)
                .Include(p => p.PollOptions)
                .Where(p => p.State == "Active"
                    && p.EndDate >= startDate
                    && p.EndDate <= endDate)
                .ToListAsync();
        }

        public async Task UpdateAsync(Poll poll)
        {
            _context.Polls.Update(poll);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var poll = await GetByIdAsync(id);
            if (poll != null)
            {
                poll.DeletedAt = DateTime.Now;
                poll.State = "Deleted";
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasUserVotedAsync(int pollId, int userId)
        {
            return await _context.Votes
                .AnyAsync(v => v.Poll_id == pollId && v.User_id == userId);
        }

        public async Task<int> GetTotalVotesAsync(int pollId)
        {
            return await _context.Votes
                .CountAsync(v => v.Poll_id == pollId);
        }

    }
}
