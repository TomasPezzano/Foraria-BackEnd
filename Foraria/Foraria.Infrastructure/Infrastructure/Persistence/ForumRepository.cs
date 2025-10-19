using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


namespace Foraria.Infrastructure.Persistence
{
    public class ForumRepository : IForumRepository
    {
        private readonly ForariaContext _context;

        public ForumRepository(ForariaContext context)
        {
            _context = context;
        }

        public async Task<Forum> Add(Forum forum)
        {
            _context.Forums.Add(forum);
            await _context.SaveChangesAsync();
            return forum;
        }

        public async Task<Forum?> GetById(int id)
        {
            return await _context.Forums
                .Include(f => f.Threads)
                .ThenInclude(t => t.Messages)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Forum>> GetAll()
        {
            return await _context.Forums
                .Include(f => f.Threads)
                .ThenInclude(t => t.Messages)
                .ToListAsync();
        }
        public async Task<Forum?> GetByCategory(ForumCategory category)
        {
            return await _context.Forums
                .FirstOrDefaultAsync(f => f.Category == category);
        }
        public async Task<Forum?> GetByIdWithThreadsAsync(int id)
        {
            return await _context.Forums
                .Include(f => f.Threads)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task Delete(int id)
        {
            var forum = await _context.Forums.FindAsync(id);
            if (forum != null)
            {
                _context.Forums.Remove(forum);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> TotalThreads(int id)
        {
            var forum = await _context.Forums
                .Include(f => f.Threads)
                .FirstOrDefaultAsync(f => f.Id == id);

            return forum?.Threads.Count ?? 0;
        }

        public async Task<int> TotalResponses(int id)
        {
            return await _context.Forums
                .Where(f => f.Id == id)
                .SelectMany(f => f.Threads)
                .SelectMany(t => t.Messages)
                .CountAsync();
        }

        public async Task<int> TotalUniqueParticipantsIncludingThreadCreators(int forumId)
        {
            var threadAuthors = _context.Forums
                .Where(f => f.Id == forumId)
                .SelectMany(f => f.Threads)
                .Select(t => t.User_id); 

            var messageAuthors = _context.Forums
                .Where(f => f.Id == forumId)
                .SelectMany(f => f.Threads)
                .SelectMany(t => t.Messages)
                .Select(m => m.User_id);

            var allParticipants = threadAuthors
                .Union(messageAuthors)
                .Distinct();

            return await allParticipants.CountAsync();
        }
}
