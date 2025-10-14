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
    }
}
