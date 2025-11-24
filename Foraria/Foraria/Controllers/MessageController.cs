using Foraria.Application.Services;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers
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
        private readonly IPermissionService _permissionService;

        public MessageController(
            CreateMessage createMessage,
            DeleteMessage deleteMessage,
            GetMessageById getMessageById,
            GetMessagesByThread getMessagesByThread,
            IWebHostEnvironment env,
            UpdateMessage updateMessage,
            HideMessage hideMessage,
            GetMessagesByUser getMessagesByUser,
            IPermissionService permissionService)
        {
            _createMessage = createMessage;
            _deleteMessage = deleteMessage;
            _getMessageById = getMessageById;
            _getMessagesByThread = getMessagesByThread;
            _env = env;
            _updateMessage = updateMessage;
            _hideMessage = hideMessage;
            _getMessagesByUser = getMessagesByUser;
            _permissionService = permissionService;
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
            await _permissionService.EnsurePermissionAsync(User, "Messages.Create");

            string? filePath = null;

            if (request.File != null)
            {
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Infrastructure/Storage/ForumFiles");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
                filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await request.File.CopyToAsync(stream);

                request.FilePath = Path.Combine("Infrastructure/Storage/ForumFiles", fileName);
            }

            var message = new Message
            {
                Content = request.Content.Trim(),
                Thread_id = request.Thread_id,
                User_id = request.User_id,
                CreatedAt = DateTime.Now,
                State = "active",
                optionalFile = filePath
            };


            var result = await _createMessage.Execute(message);

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
            await _permissionService.EnsurePermissionAsync(User, "Messages.View");

            var message = await _getMessageById.Execute(id);
            if (message == null) return NotFound();

            var response = new MessageResponse
            {
                Id = message.Id,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                State = message.State,
                Thread_id = message.Thread_id,
                User_id = message.User_id,
                UserFirstName = message.User.Name,
                UserLastName = message.User.LastName,
                optionalFile = message.optionalFile
            };
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
            await _permissionService.EnsurePermissionAsync(User, "Messages.ViewByThread");

            var messages = await _getMessagesByThread.Execute(threadId);

            var responses = messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                State = m.State,
                Thread_id = m.Thread_id,
                User_id = m.User_id,
                UserFirstName = m.User.Name ,
                UserLastName = m.User.LastName ,
                optionalFile = m.optionalFile
            });

            return Ok(responses);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Elimina un mensaje existente.",
            Description = "Elimina un mensaje del foro pero no de la base de datos. Los administradores pueden borrar cualquier mensaje. Los usuarios solo pueden borrar su propio mensaje"
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, [FromQuery] int userId)
        {
            await _permissionService.EnsurePermissionAsync(User, "Messages.DeleteOwn");

            await _deleteMessage.ExecuteAsync(id, userId);
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Actualiza el contenido de un mensaje.",
            Description = "Permite modificar el texto de un mensaje y, opcionalmente, añadir un archivo adjunto."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMessageRequest request)
        {
            await _permissionService.EnsurePermissionAsync(User, "Messages.UpdateOwn");

            string? filePath = null;

            if (request.File != null)
            {
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Infrastructure/Storage/ForumFiles");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
                var savedPath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(savedPath, FileMode.Create);
                await request.File.CopyToAsync(stream);

                filePath = Path.Combine("Infrastructure/Storage/ForumFiles", fileName);
            }

            var messageToUpdate = new Message
            {
                Id = id,
                Content = request.Content,
                optionalFile = filePath
            };

            var userId = request.UserId;

            var updatedMessage = await _updateMessage.ExecuteAsync(messageToUpdate, userId);

            return Ok(new
            {
                updatedMessage.Id,
                updatedMessage.Content,
                updatedMessage.optionalFile,
                updatedMessage.UpdatedAt
            });
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
            await _permissionService.EnsurePermissionAsync(User, "Messages.Hide");

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
            await _permissionService.EnsurePermissionAsync(User, "Messages.ViewByUser");

            var messages = await _getMessagesByUser.ExecuteAsync(userId);

            var response = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                State = m.State,
                OptionalFile = m.optionalFile,
                UserId = m.User_id
            });

            return Ok(response);
        }
    }
}
