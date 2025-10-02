using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly CreateMessage _createMessage;
        private readonly DeleteMessage _deleteMessage;
        private readonly IMessageRepository _repository;
        private readonly IWebHostEnvironment _env;

        public MessagesController(CreateMessage createMessage, DeleteMessage deleteMessage, IMessageRepository repository, IWebHostEnvironment env)
        {
            _createMessage = createMessage;
            _deleteMessage = deleteMessage;
            _repository = repository;
            _env = env;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateMessageWithFileRequest request)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool isAdmin = true; //temp hasta tener auth

            if (!isAdmin)
                return Forbid();

            var deleted = await _deleteMessage.Execute(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}