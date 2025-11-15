using Foraria.Application.Services;
using Foraria.Domain.Repository;
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
    public class ReactionsController : ControllerBase
    {
        private readonly ToggleReaction _toggleReaction;
        private readonly IReactionRepository _repository;
        private readonly IPermissionService _permissionService;

        public ReactionsController(
            ToggleReaction toggleReaction,
            IReactionRepository repository,
            IPermissionService permissionService)
        {
            _toggleReaction = toggleReaction;
            _repository = repository;
            _permissionService = permissionService;
        }


        [HttpPost("toggle")]
        [Authorize(Policy = "OwnerAndTenant")]
        [SwaggerOperation(
            Summary = "Agrega o quita una reacción (like/dislike).",
            Description = "Permite alternar una reacción en un mensaje o hilo específico. Si la reacción ya existe, se elimina; si no, se crea."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Toggle([FromBody] ReactionRequest request)
        {
            await _permissionService.EnsurePermissionAsync(User, "Reactions.Toggle");

            if (request == null)
                throw new ValidationException("La solicitud no puede estar vacía.");

            if (request.User_id <= 0)
                throw new ValidationException("Debe especificar un ID de usuario válido.");

            if (request.ReactionType == null)
                throw new ValidationException("Debe especificar el tipo de reacción.");

            // debe apuntar a mensaje o hilo, al menos uno
            if (request.Message_id == null && request.Thread_id == null)
                throw new ValidationException("Debe asociar la reacción a un mensaje o a un hilo.");

            try
            {
                var result = await _toggleReaction.Execute(
                    request.User_id,
                    request.Message_id,
                    request.Thread_id,
                    request.ReactionType
                );

                if (!result)
                    throw new ReactionOperationException("No se pudo registrar o eliminar la reacción.");

                return Ok(new { success = result });
            }
            catch (InvalidOperationException ex)
            {
                throw new ReactionNotAllowedException(ex.Message);
            }
        }

        [HttpGet("message/{messageId}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene las reacciones de un mensaje.",
            Description = "Devuelve la cantidad total de reacciones (likes y dislikes) aplicadas a un mensaje específico del foro."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMessageReactions(int messageId)
        {
            await _permissionService.EnsurePermissionAsync(User, "Reactions.ViewMessage");

            if (messageId <= 0)
                throw new ValidationException("Debe proporcionar un ID de mensaje válido.");

            var total = await _repository.CountByMessage(messageId);

            if (total == 0)
                throw new NotFoundException($"No se encontraron reacciones para el mensaje con ID {messageId}.");

            var likes = await _repository.CountLikesByMessage(messageId);
            var dislikes = await _repository.CountDislikesByMessage(messageId);

            return Ok(new ReactionResponse
            {
                Total = total,
                Likes = likes,
                Dislikes = dislikes
            });
        }

        [HttpGet("thread/{threadId}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene las reacciones de un hilo.",
            Description = "Devuelve el total de reacciones (likes y dislikes) asociadas a un hilo completo del foro."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetThreadReactions(int threadId)
        {
            await _permissionService.EnsurePermissionAsync(User, "Reactions.ViewThread");

            if (threadId <= 0)
                throw new ValidationException("Debe proporcionar un ID de hilo válido.");

            var total = await _repository.CountByThread(threadId);

            if (total == 0)
                throw new NotFoundException($"No se encontraron reacciones para el hilo con ID {threadId}.");

            var likes = await _repository.CountLikesByThread(threadId);
            var dislikes = await _repository.CountDislikesByThread(threadId);

            return Ok(new ReactionResponse
            {
                Total = total,
                Likes = likes,
                Dislikes = dislikes
            });
        }
    }
}
