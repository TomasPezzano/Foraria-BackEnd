using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

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
        private readonly IWebHostEnvironment _env;

        public MessageController(
            CreateMessage createMessage,
            DeleteMessage deleteMessage,
            GetMessageById getMessageById,
            GetMessagesByThread getMessagesByThread,
            IWebHostEnvironment env)
        {
            _createMessage = createMessage;
            _deleteMessage = deleteMessage;
            _getMessageById = getMessageById;
            _getMessagesByThread = getMessagesByThread;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateMessageWithFileRequest request)
        {
            if (request.File != null)
            {
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Infrastructure/Storage/ForumFiles");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await request.File.CopyToAsync(stream);

                request.FilePath = Path.Combine("Infrastructure/Storage/ForumFiles", fileName);
            }

            var message = await _createMessage.Execute(request);
            return Ok(message);
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
