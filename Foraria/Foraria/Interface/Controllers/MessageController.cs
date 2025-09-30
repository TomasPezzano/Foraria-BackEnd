namespace Foraria.Interface.Controllers
{
    using System;
    using ForariaDomain;
    using Foraria.Domain.Repository;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Foraria.Infrastructure.Persistence;

    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ForariaContext _context;

        public MessagesController(ForariaContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] Message message)
        {
            var thread = await _context.Threads.FindAsync(message.Thread_id);
            if (thread == null)
                return NotFound("El hilo no existe.");

            message.CreatedAt = DateTime.UtcNow;
            message.State = "active";

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(message);
        }

        [HttpGet("thread/{threadId}")]
        public async Task<IActionResult> GetThreadWithMessages(int threadId)
        {
            var thread = await _context.Threads
                .Include(t => t.Messages)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(t => t.Id == threadId);

            if (thread == null) return NotFound();

            return Ok(thread);
        }
    }


}


