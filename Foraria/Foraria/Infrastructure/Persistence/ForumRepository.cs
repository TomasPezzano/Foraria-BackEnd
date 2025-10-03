using Foraria.Domain.Repository;
using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


namespace Foraria.Infrastructure.Repository
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
    }
}
