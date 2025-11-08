using Foraria.Application.UseCase;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class VotesController : ControllerBase
    {
        private readonly CreateVote _createVoteUseCase;

        public VotesController(CreateVote createVoteUseCase)
        {
            _createVoteUseCase = createVoteUseCase;
        }

        [HttpPost]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Registra un voto en una encuesta.",
            Description = "Permite a un usuario emitir su voto en una encuesta activa, indicando la opción seleccionada.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Voto registrado correctamente")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos o encuesta no disponible")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Encuesta u opción no encontrada")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Error interno del servidor")]
        public async Task<IActionResult> PostVote([FromBody] VoteDto request)
        {
            if (!ModelState.IsValid)
                throw new DomainValidationException("Los datos del voto son inválidos.");

            if (request.User_Id <= 0 || request.Poll_Id <= 0 || request.PollOption_Id <= 0)
                throw new DomainValidationException("Debe especificar IDs válidos para usuario, encuesta y opción.");

            var vote = new Vote
            {
                User_id = request.User_Id,
                Poll_id = request.Poll_Id,
                PollOption_id = request.PollOption_Id,
                VotedDate = DateTime.UtcNow
            };

            await _createVoteUseCase.ExecuteAsync(vote);

            return Ok(new { message = "Voto registrado correctamente." });
        }
    }
}
