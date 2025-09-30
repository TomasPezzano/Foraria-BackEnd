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
    }
}
