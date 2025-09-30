using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foraria.Infrastructure.Repository
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

        public async Task<Message> GetById(int id)
        {
            return await _context.Messages
                .Include(m => m.User)
                .Include(m => m.Thread)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Message>> GetByThreadId(int threadId)
        {
            return await _context.Messages
                .Include(m => m.User)
                .Where(m => m.Thread_id == threadId)
                .ToListAsync();
        }
    }
}