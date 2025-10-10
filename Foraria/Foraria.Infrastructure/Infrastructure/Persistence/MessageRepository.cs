using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ForariaContext _context;

        public MessageRepository(ForariaContext context)
        {
            _context = context;
        }

        public async Task<Message> Add(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<Message?> GetById(int id)
        {
            return await _context.Messages
                .Include(m => m.User)
                .Include(m => m.Thread)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Message>> GetByThread(int threadId)
        {
            return await _context.Messages
                .Where(m => m.Thread_id == threadId)
                .Include(m => m.User)
                .Include(m => m.Thread)
                .ToListAsync();
        }

        public async Task Delete(Message message)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }
    }
}