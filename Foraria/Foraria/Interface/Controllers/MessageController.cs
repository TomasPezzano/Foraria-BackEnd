using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Crea un nuevo mensaje dentro de un hilo del foro.",
            Description = "Permite crear un mensaje asociado a un hilo existente. Si se incluye un archivo, será almacenado en la carpeta de foro correspondiente."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene un mensaje por su ID.",
            Description = "Devuelve la información completa de un mensaje específico, incluyendo su contenido y cualquier archivo adjunto."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _getMessageById.Execute(id);
            if (response == null) return NotFound();
            return Ok(response);
        }

        [HttpGet("thread/{threadId}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene los mensajes asociados a un hilo.",
            Description = "Devuelve todos los mensajes publicados dentro del hilo especificado, ordenados cronológicamente."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByThread(int threadId)
        {
            var responses = await _getMessagesByThread.Execute(threadId);
            return Ok(responses);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Elimina un mensaje existente.",
            Description = "Elimina un mensaje del foro de forma permanente. Solo los administradores o el autor pueden realizar esta acción."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _deleteMessage.Execute(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize (Policy = "All")]
        [SwaggerOperation(
            Summary = "Actualiza el contenido de un mensaje.",
            Description = "Permite modificar el texto de un mensaje y, opcionalmente, añadir un archivo adjunto."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [Authorize (Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Oculta un mensaje sin eliminarlo.",
            Description = "Cambia el estado de visibilidad del mensaje, ocultándolo del foro sin eliminarlo de la base de datos."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [Authorize (Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Obtiene todos los mensajes publicados por un usuario.",
            Description = "Devuelve la lista completa de mensajes creados por el usuario especificado en todos los hilos en los que haya participado."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var messages = await _getMessagesByUser.ExecuteAsync(userId);
            return Ok(messages);
        }
    }
}
