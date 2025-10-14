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
        private readonly UpdateMessage _updateMessage;
        private readonly HideMessage _hideMessage;
        private readonly GetMessagesByUser _getMessagesByUser;

        public MessageController(
            CreateMessage createMessage,
            DeleteMessage deleteMessage,
            GetMessageById getMessageById,
            GetMessagesByThread getMessagesByThread,
            IWebHostEnvironment env,
            UpdateMessage updateMessage,
            HideMessage hideMessage,
            GetMessagesByUser getMessagesByUser)
        {
            _createMessage = createMessage;
            _deleteMessage = deleteMessage;
            _getMessageById = getMessageById;
            _getMessagesByThread = getMessagesByThread;
            _env = env;
            _updateMessage = updateMessage;
            _hideMessage = hideMessage;
            _getMessagesByUser = getMessagesByUser;
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateMessageRequest request)
        {
            if (request.File != null)
            {
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Infrastructure/Storage/ForumFiles");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await request.File.CopyToAsync(stream);

                request.FilePathToUpdate = Path.Combine("Infrastructure/Storage/ForumFiles", fileName);
            }

            var updated = await _updateMessage.ExecuteAsync(id, request);
            return Ok(updated);
        }

        [HttpPatch("{id}/hide")]
        public async Task<IActionResult> Hide(int id)
        {
            try
            {
                await _hideMessage.ExecuteAsync(id);
                return Ok(new { message = "Mensaje ocultado correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var messages = await _getMessagesByUser.ExecuteAsync(userId);
            return Ok(messages);
        }
    }
}
