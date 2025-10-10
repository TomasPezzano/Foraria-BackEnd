using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence
{
    public class ReactionRepository : IReactionRepository
    {
        private readonly ForariaContext _context;

        public ReactionRepository(ForariaContext context)
        {
            _context = context;
        }

        public async Task<Reaction?> GetById(int id) =>
            await _context.Reactions.FindAsync(id);

        public async Task Add(Reaction reaction)
        {
            _context.Reactions.Add(reaction);
            await _context.SaveChangesAsync();
        }

        public async Task Remove(Reaction reaction)
        {
            _context.Reactions.Remove(reaction);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountByMessage(int messageId) =>
            await _context.Reactions
                .Where(r => r.Message_id == messageId)
                .SumAsync(r => r.ReactionType);

        public async Task<int> CountByThread(int threadId) =>
            await _context.Reactions
                .Where(r => r.Thread_id == threadId)
                .SumAsync(r => r.ReactionType);

        public async Task<Reaction?> GetByUserAndTarget(int userId, int? messageId, int? threadId) =>
            await _context.Reactions
                .FirstOrDefaultAsync(r =>
                    r.User_id == userId &&
                    r.Message_id == messageId &&
                    r.Thread_id == threadId);
    }
}
