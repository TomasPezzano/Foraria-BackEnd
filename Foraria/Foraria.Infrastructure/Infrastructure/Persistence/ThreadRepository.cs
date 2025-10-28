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
            .Include(t => t.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    public async Task<IEnumerable<Thread>> GetAllAsync()
    {
        return await _context.Threads
            .Include(t => t.User)
            .Include(t => t.Forum)
            .ToListAsync();
    }

    public async Task<IEnumerable<Thread>> GetByForumIdAsync(int forumId)
    {
        return await _context.Threads
            .Where(t => t.ForumId == forumId)
            .Include(t => t.User)
            .Include(t => t.Forum)
            .ToListAsync();
    }

    public async Task UpdateAsync(Thread thread)
    {
        _context.Threads.Update(thread);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var thread = await _context.Threads.FindAsync(id);
        if (thread != null)
        {
            _context.Threads.Remove(thread);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<Thread?> GetByIdWithMessagesAsync(int id)
    {
        return await _context.Threads
            .Include(t => t.Messages)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

}

