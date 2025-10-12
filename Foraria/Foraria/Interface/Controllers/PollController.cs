using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/polls")]
    public class PollController : Controller
    {
        private readonly CreatePoll _createPoll;
        private readonly GetPolls _polls;
        private readonly NotarizePoll _notarizePoll; 
        private readonly GetPollById _getPollById;
        public PollController(CreatePoll poll, GetPolls polls, GetPollById getPollById, NotarizePoll notarizePoll)
        {
            _createPoll = poll;
            _polls = polls;
            _getPollById = getPollById;
            _notarizePoll = notarizePoll;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PollDto request)
        {
            try
            {
                var poll = new Poll
                {
                    Title = request.Title,
                    Description = request.Description,
                    CategoryPoll_id = request.CategoryPollId,
                    User_id = request.UserId,
                    CreatedAt = DateTime.UtcNow,
                    State = "Activa",
                    PollOptions = request.Options.Select(optionText => new PollOption
                    {
                        Text = optionText
                    }).ToList()
                };

                var result = await _createPoll.ExecuteAsync(poll);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrió un error inesperado", detail = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<Poll> polls = await _polls.ExecuteAsync();

            List<PollDto> pollsDto = polls.Select(p => new PollDto
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var poll = await _getPollById.ExecuteAsync(id);
            if (poll == null)
                return NotFound();

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
        public async Task<IActionResult> Notarize(int id)
        {
            var poll = await _getPollById.ExecuteAsync(id);
            if (poll == null)
                return NotFound(new { error = "La votación no existe." });

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
    }



}
