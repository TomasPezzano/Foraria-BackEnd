using Foraria.Application.UseCase;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Foraria.Controllers
{
    [ApiController]
    [Route("api/polls")]
    public class PollController : Controller
    {
        private readonly CreatePoll _createPoll;
        private readonly GetPolls _polls;
        private readonly NotarizePoll _notarizePoll;
        private readonly GetPollById _getPollById;
        private readonly GetPollWithResults _getPollWithResults;
        private readonly GetAllPollsWithResults _getAllPollsWithResults;
        private readonly GetActivePollCount _getActivePollCount;
        private readonly ChangePollState _changePollState;
        private readonly UpdatePoll _updatePoll;

        public PollController(
            CreatePoll poll,
            GetPolls polls,
            GetPollById getPollById,
            NotarizePoll notarizePoll,
            GetPollWithResults getPollWithResults,
            GetAllPollsWithResults getAllPollsWithResults,
            GetActivePollCount getActivePollCount,
            ChangePollState changePollState,
            UpdatePoll updatePoll)
        {
            _createPoll = poll;
            _polls = polls;
            _getPollById = getPollById;
            _notarizePoll = notarizePoll;
            _getPollWithResults = getPollWithResults;
            _getAllPollsWithResults = getAllPollsWithResults;
            _getActivePollCount = getActivePollCount;
            _changePollState = changePollState;
            _updatePoll = updatePoll;
        }

        [HttpPost]
        [Authorize(Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Crea una nueva votación.",
            Description = "Permite crear una nueva votación con opciones y fechas de expiración, asignada a un usuario y categoría."
        )]
        [ProducesResponseType(typeof(Poll), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] PollDto request)
        {
            if (request == null)
                throw new DomainValidationException("El cuerpo de la solicitud no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new DomainValidationException("El título de la votación es obligatorio.");

            if (request.Options == null || !request.Options.Any())
                throw new DomainValidationException("Debe incluir al menos una opción para la votación.");

            var poll = new Poll
            {
                Title = request.Title,
                Description = request.Description,
                CategoryPoll_id = request.CategoryPollId,
                User_id = request.UserId,
                CreatedAt = DateTime.UtcNow,
                DeletedAt = DateTime.UtcNow.AddDays(7),
                State = "Activa",
                PollOptions = request.Options.Select(optionText => new PollOption
                {
                    Text = optionText
                }).ToList()
            };

            var result = await _createPoll.ExecuteAsync(poll);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene todas las votaciones.",
            Description = "Devuelve una lista con todas las votaciones activas o finalizadas del sistema."
        )]
        [ProducesResponseType(typeof(IEnumerable<PollDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            var polls = await _polls.ExecuteAsync();

            if (polls == null || !polls.Any())
                throw new NotFoundException("No se encontraron votaciones.");

            var pollsDto = polls.Select(p => new PollDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                CategoryPollId = p.CategoryPoll_id,
                CreatedAt = p.CreatedAt,
                DeletedAt = p.DeletedAt,
                State = p.State,
                UserId = p.User_id,
                Options = p.PollOptions != null
                           ? p.PollOptions.Select(o => o.Text).ToList()
                           : new List<string>()
            }).ToList();

            return Ok(pollsDto);
        }

        [HttpGet("with-results/{id}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene una votación con sus resultados.",
            Description = "Devuelve los detalles de una votación específica junto con la cantidad de votos por opción."
        )]
        [ProducesResponseType(typeof(PollWithResultsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPollWithResults(int id)
        {
            var poll = await _getPollWithResults.ExecuteAsync(id);

            if (poll == null)
                throw new NotFoundException("Encuesta no encontrada.");

            var pollResults = poll.Votes
                .GroupBy(v => v.PollOption_id)
                .Select(g => new PollResultDto
                {
                    PollOptionId = g.Key,
                    VotesCount = g.Count()
                })
                .ToList();

            var pollDto = new PollWithResultsDto
            {
                Id = poll.Id,
                Title = poll.Title,
                Description = poll.Description,
                CreatedAt = poll.CreatedAt,
                DeletedAt = poll.DeletedAt,
                State = poll.State,
                CategoryPollId = poll.CategoryPoll_id,
                PollOptions = poll.PollOptions.Select(option => new PollOptionDto
                {
                    Id = option.Id,
                    Text = option.Text
                }).ToList(),
                PollResults = pollResults
            };

            return Ok(pollDto);
        }

        [HttpGet("with-results")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene todas las votaciones con resultados.",
            Description = "Devuelve todas las votaciones existentes con su información agregada de resultados."
        )]
        [ProducesResponseType(typeof(IEnumerable<PollWithResultsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPollsWithResults()
        {
            var polls = await _getAllPollsWithResults.ExecuteAsync();

            if (polls == null || !polls.Any())
                throw new NotFoundException("No se encontraron encuestas.");

            var pollsDto = polls.Select(poll => new PollWithResultsDto
            {
                Id = poll.Id,
                Title = poll.Title,
                Description = poll.Description,
                CreatedAt = poll.CreatedAt,
                DeletedAt = poll.DeletedAt,
                State = poll.State,
                CategoryPollId = poll.CategoryPoll_id,
                PollOptions = poll.PollOptions.Select(option => new PollOptionDto
                {
                    Id = option.Id,
                    Text = option.Text
                }).ToList(),
                PollResults = poll.Votes
                    .GroupBy(v => v.PollOption_id)
                    .Select(g => new PollResultDto
                    {
                        PollOptionId = g.Key,
                        VotesCount = g.Count()
                    }).ToList()
            }).ToList();

            return Ok(pollsDto);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene una votación por ID.",
            Description = "Devuelve los datos de una votación específica."
        )]
        [ProducesResponseType(typeof(PollDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var poll = await _getPollById.ExecuteAsync(id);
            if (poll == null)
                throw new NotFoundException("La votación solicitada no existe.");

            var pollReceived = new PollDto
            {
                Id = poll.Id,
                Title = poll.Title,
                Description = poll.Description,
                CategoryPollId = poll.CategoryPoll_id,
                CreatedAt = poll.CreatedAt,
                DeletedAt = poll.DeletedAt,
                State = poll.State,
                UserId = poll.User_id,
                Options = poll.PollOptions != null
                    ? poll.PollOptions.Select(option => option.Text).ToList()
                    : new List<string>()
            };

            return Ok(pollReceived);
        }

        [HttpPost("{id:int}/notarize")]
        [Authorize(Policy = "OwnerAndTenant")]
        [SwaggerOperation(
            Summary = "Notariza una votación en blockchain.",
            Description = "Registra en la blockchain la información de una votación para garantizar su integridad."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Notarize(int id)
        {
            var poll = await _getPollById.ExecuteAsync(id);
            if (poll == null)
                throw new NotFoundException("La votación no existe.");

            var text = $"{poll.Title} - {poll.Description}";
            var proof = await _notarizePoll.ExecuteAsync(id, text);

            return Ok(new
            {
                message = "Votación registrada en blockchain correctamente.",
                txHash = proof.TxHash,
                hashHex = proof.HashHex,
                link = $"https://amoy.polygonscan.com/tx/{proof.TxHash}"
            });
        }

        [HttpGet("polls/active-count")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene la cantidad de votaciones activas.",
            Description = "Cuenta las votaciones activas en el consorcio indicado y devuelve la fecha de chequeo."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivePollCount([FromQuery] int consortiumId, [FromQuery] DateTime? dateTime = null)
        {
            if (consortiumId <= 0)
                throw new DomainValidationException("Debe indicar un ID de consorcio válido.");

            var count = await _getActivePollCount.ExecuteAsync(consortiumId, dateTime);

            return Ok(new
            {
                activePolls = count,
                checkedAt = (dateTime ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        [HttpPost("{pollId}/state")]
        [Authorize(Roles = "Consorcio")]
        [SwaggerOperation(
            Summary = "Cambia el estado de una votación",
            Description = "Permite aprobar o rechazar una votación pendiente. Solo el rol Consorcio puede ejecutar esta acción.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Estado de la votación actualizado correctamente", typeof(Poll))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Solicitud inválida o estado no permitido")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "El usuario no tiene permisos para cambiar el estado de la votación")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "La votación o el usuario no fueron encontrados")]
        public async Task<IActionResult> ChangePollState(int pollId, [FromBody] ChangePollStateRequest request)
        {
            if (pollId <= 0)
                throw new DomainValidationException("El ID de la votación debe ser válido.");

            var result = await _changePollState.ExecuteAsync(pollId, request.UserId, request.NewState);
            return Ok(result);
        }

        [HttpPut("{pollId}")]
        [Authorize(Roles = "Administrador,Consorcio")]
        [SwaggerOperation(
            Summary = "Actualiza los datos de una votación existente",
            Description = "Permite modificar el título, descripción, fechas o estado de una votación. Solo los roles Administrador o Consorcio pueden editar.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Votación actualizada correctamente", typeof(Poll))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos o error de validación")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "El usuario no tiene permisos para modificar la votación")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "La votación no fue encontrada")]
        public async Task<IActionResult> UpdatePoll(int pollId, [FromBody] UpdatePollRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedException("Token inválido.");

            var pollData = new Poll
            {
                Title = request.Title,
                Description = request.Description,
                State = request.State,
                StartDate = request.StartDate ?? default,
                EndDate = request.EndDate ?? default,
            };

            var result = await _updatePoll.ExecuteAsync(pollId, userId, pollData);
            return Ok(result);
        }
    }
}
