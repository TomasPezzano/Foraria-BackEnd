using Foraria.Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;
using Thread = ForariaDomain.Thread;


    public class ThreadRepository : IThreadRepository
    {
        private readonly ForariaContext _context;

        public ThreadRepository(ForariaContext context)
        {
            _context = context;
        }

        public async Task Add(Thread thread)
        {
            _context.Threads.Add(thread);
            await _context.SaveChangesAsync();
        }

        public async Task<Thread?> GetById(int id)
        {
            return await _context.Threads
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

    }

