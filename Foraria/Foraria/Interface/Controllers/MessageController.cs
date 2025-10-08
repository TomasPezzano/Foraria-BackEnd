using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly CreateMessage _createMessage;
        private readonly DeleteMessage _deleteMessage;
        private readonly GetMessageById _getMessageById;
        private readonly GetMessagesByThread _getMessagesByThread;

        public MessageController(
            CreateMessage createMessage,
            DeleteMessage deleteMessage,
            GetMessageById getMessageById,
            GetMessagesByThread getMessagesByThread)
        {
            _createMessage = createMessage;
            _deleteMessage = deleteMessage;
            _getMessageById = getMessageById;
            _getMessagesByThread = getMessagesByThread;
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
            var response = await _getMessageById.Execute(id);
            if (response == null) return NotFound();
            return Ok(response);
        }

        [HttpGet("thread/{threadId}")]
        public async Task<IActionResult> GetByThread(int threadId)
        {
            var responses = await _getMessagesByThread.Execute(threadId);
            return Ok(responses);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _deleteMessage.Execute(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
