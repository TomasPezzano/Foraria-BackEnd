using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly CreateMessage _createMessage;
        private readonly IMessageRepository _repository;

        public MessagesController(CreateMessage createMessage, IMessageRepository repository)
        {
            _createMessage = createMessage;
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMessageRequest request)
        {
            var created = await _createMessage.Execute(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var message = await _repository.GetById(id);
            if (message == null) return NotFound();

            var response = new MessageResponse
            {
                Id = message.Id,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                State = message.State,
                Thread_id = message.Thread_id,
                User_id = message.User_id,
                optionalFile = message.optionalFile
            };

            return Ok(response);
        }

        [HttpGet("thread/{threadId}")]
        public async Task<IActionResult> GetByThread(int threadId)
        {
            var messages = await _repository.GetByThread(threadId);

            var response = messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                State = m.State,
                Thread_id = m.Thread_id,
                User_id = m.User_id,
                optionalFile = m.optionalFile
            });

            return Ok(response);
        }
    }
}
