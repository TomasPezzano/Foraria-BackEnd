using Foraria.Application.Services;
using Foraria.DTOs;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThreadController : ControllerBase
    {
        private readonly CreateThread _createThread;
        private readonly GetThreadById _getThreadById;
        private readonly GetAllThreads _getAllThreads;
        private readonly DeleteThread _deleteThread;
        private readonly UpdateThread _updateThread;
        private readonly GetThreadWithMessages _getThreadWithMessages;
        private readonly CloseThread _closeThread;
        private readonly GetThreadCommentCount _getThreadCommentCount;
        private readonly IPermissionService _permissionService;

        public ThreadController(
            CreateThread createThread,
            GetThreadById getThreadById,
            GetAllThreads getAllThreads,
            DeleteThread deleteThread,
            UpdateThread updateThread,
            GetThreadWithMessages getThreadWithMessages,
            CloseThread closeThread,
            GetThreadCommentCount getThreadCommentCount,
            IPermissionService permissionService)
        {
            _createThread = createThread;
            _getThreadById = getThreadById;
            _getAllThreads = getAllThreads;
            _deleteThread = deleteThread;
            _updateThread = updateThread;
            _getThreadWithMessages = getThreadWithMessages;
            _closeThread = closeThread;
            _getThreadCommentCount = getThreadCommentCount;
            _permissionService = permissionService;
        }


        [HttpPost]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Crea un nuevo hilo de discusión.",
            Description = "Permite crear un nuevo hilo dentro de un foro existente, asociado a un usuario y una categoría. Devuelve los datos del hilo creado."
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateThreadRequest request)
        {
            await _permissionService.EnsurePermissionAsync(User, "Threads.Create");

            if (request == null)
                throw new ValidationException("El cuerpo de la solicitud no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(request.Theme))
                throw new ValidationException("El título del hilo es obligatorio.");

            if (request.UserId <= 0)
                throw new ValidationException("Debe especificar un usuario válido.");

            if (request.ForumId <= 0)
                throw new ValidationException("Debe especificar un foro válido.");

            var thread = new ForariaDomain.Thread
            {
                Theme = request.Theme.Trim(),
                Description = request.Description.Trim(),
                ForumId = request.ForumId,
                UserId = request.UserId,
                CreatedAt = DateTime.Now,
                State = "Active"
            };

            var createdThread = await _createThread.Execute(thread);

            if (createdThread == null)
                throw new BusinessException("No se pudo crear el hilo. Verifique los datos ingresados.");

            return CreatedAtAction(nameof(GetById), new { id = createdThread.Id }, new ThreadResponse
            {
                Id = createdThread.Id,
                Theme = createdThread.Theme,
                Description = createdThread.Description,
                Forum_id = createdThread.ForumId,
                User_id = createdThread.UserId,
                CreatedAt = createdThread.CreatedAt,
                State = createdThread.State
            });
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene un hilo por su ID.",
            Description = "Devuelve los detalles del hilo solicitado, incluyendo información básica como su tema, estado y usuario creador."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            await _permissionService.EnsurePermissionAsync(User, "Threads.View");

            if (id <= 0)
                throw new ValidationException("El ID del hilo debe ser mayor que cero.");

            var threadResponse = await _getThreadById.Execute(id);

            if (threadResponse == null)
                throw new ThreadNotFoundException($"No se encontró el hilo con ID {id}.");


            var response = new ThreadResponse
            {
                Id = threadResponse.Id,
                Theme = threadResponse.Theme,
                Description = threadResponse.Description,
                CreatedAt = threadResponse.CreatedAt,
                State = threadResponse.State,
                Forum_id = threadResponse.ForumId,
                User_id = threadResponse.UserId
            };

            return Ok(response);
        }

        [HttpGet]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene todos los hilos de un foro.",
            Description = "Devuelve una lista de hilos."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] int? forumId)
        {
            await _permissionService.EnsurePermissionAsync(User, "Threads.ViewAll");

            if (forumId is < 0)
                throw new ValidationException("El ID de foro no puede ser negativo.");

            var threads = await _getAllThreads.ExecuteAsync(forumId);

            if (threads == null || !threads.Any())
                throw new NotFoundException("No se encontraron hilos para este foro.");

            var result = threads.Select(t => new ThreadDto
            {
                Id = t.Id,
                Theme = t.Theme,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                State = t.State,
                UserId = t.UserId,
                UserName = t.User.Name,
                UserLastName = t.User.LastName,
                ForumId = t.ForumId
            });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Elimina un hilo existente.",
            Description = "Elimina un hilo."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _permissionService.EnsurePermissionAsync(User, "Threads.Delete");

            if (id <= 0)
                throw new ValidationException("Debe especificar un ID de hilo válido.");

            try
            {
                await _deleteThread.ExecuteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessException(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Actualiza la información de un hilo.",
            Description = "Permite modificar el título, descripción u otros campos del hilo indicado."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateThreadRequest request)
        {
            await _permissionService.EnsurePermissionAsync(User, "Threads.Update");

            if (id <= 0)
                throw new ValidationException("Debe especificar un ID de hilo válido.");

            if (request == null)
                throw new ValidationException("Debe proporcionar datos para actualizar el hilo.");

            var threadToUpdate = new ForariaDomain.Thread
            {
                Theme = request.Theme,
                Description = request.Description,
                State = request.State,
                UserId = request.UserId
            };


            try
            {
                var result = await _updateThread.ExecuteAsync(id, threadToUpdate);
                if (result == null)
                    throw new ThreadNotFoundException($"No se encontró el hilo con ID {id} para actualizar.");

                var response = new ThreadDto
                {
                    Id = result.Id,
                    Theme = result.Theme,
                    Description = result.Description,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt,
                    State = result.State,
                    UserId = result.UserId,
                    ForumId = result.ForumId
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                throw new ThreadUpdateException(ex.Message);
            }

        }

        [HttpGet("{id}/messages")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene un hilo con todos sus mensajes.",
            Description = "Devuelve el hilo solicitado junto con todos los mensajes y respuestas asociados."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetThreadWithMessages(int id)
        {
            await _permissionService.EnsurePermissionAsync(User, "Threads.ViewWithMessages");

            if (id <= 0)
                throw new ValidationException("El ID del hilo debe ser mayor que cero.");

            var thread = await _getThreadWithMessages.ExecuteAsync(id);

            if (thread == null)
                throw new ThreadNotFoundException($"No se encontró el hilo con ID {id}.");

            var result = new ThreadWithMessagesDto
            {
                Id = thread.Id,
                Theme = thread.Theme,
                Description = thread.Description,
                CreatedAt = thread.CreatedAt,
                State = thread.State,
                UserId = thread.UserId,
                ForumId = thread.ForumId,
                Messages = thread.Messages.Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt,
                    State = m.State,
                    OptionalFile = m.optionalFile,
                    UserId = m.User_id
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPatch("{id}/close")]
        [Authorize(Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Cierra un hilo de discusión.",
            Description = "Marca el hilo como cerrado, impidiendo que se agreguen nuevos mensajes o reacciones. Solo el creador o un administrador pueden cerrarlo."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Close(int id)
        {
            await _permissionService.EnsurePermissionAsync(User, "Threads.Close");

            if (id <= 0)
                throw new ValidationException("Debe especificar un ID de hilo válido.");

            var thread = await _closeThread.ExecuteAsync(id);

            var response = new ThreadDto
            {
                Id = thread.Id,
                Theme = thread.Theme,
                Description = thread.Description,
                CreatedAt = thread.CreatedAt,
                State = thread.State,
                UserId = thread.UserId,
                ForumId = thread.ForumId
            };

            return Ok(response);
        }

        [HttpGet("{threadId}/comment-count")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene la cantidad total de comentarios de un hilo.",
            Description = "Devuelve el número total de mensajes o comentarios asociados a un hilo específico."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCommentCount(int threadId)
        {
            await _permissionService.EnsurePermissionAsync(User, "Threads.ViewCommentCount");

            if (threadId <= 0)
                throw new ValidationException("Debe especificar un ID de hilo válido.");

            try
            {
                var count = await _getThreadCommentCount.Execute(threadId);
                return Ok(new { ThreadId = threadId, CommentCount = count });
            }
            catch (InvalidOperationException ex)
            {
                throw new ThreadNotFoundException(ex.Message);
            }
        }
    }
}
