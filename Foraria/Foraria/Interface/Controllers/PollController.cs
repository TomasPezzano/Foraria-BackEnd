using Foraria.Application.UseCase;
using Foraria.Contracts.DTOs;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
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
        private readonly GetPollWithResults _getPollWithResults;
        private readonly GetAllPollsWithResults _getAllPollsWithResults;
        private readonly GetActivePollCount _getActivePollCount;
        public PollController(CreatePoll poll, GetPolls polls, GetPollById getPollById, NotarizePoll notarizePoll,GetPollWithResults getPollWithResults, GetAllPollsWithResults getAllPollsWithResults, GetActivePollCount getActivePollCount)
        {
            _createPoll = poll;
            _polls = polls;
            _getPollById = getPollById;
            _notarizePoll = notarizePoll;
            _getPollWithResults = getPollWithResults;
            _getAllPollsWithResults = getAllPollsWithResults;
            _getActivePollCount = getActivePollCount;
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

        [HttpGet("with-results/{id}")]
        public async Task<IActionResult> GetPollWithResults(int id)
        {
            var poll = await _getPollWithResults.ExecuteAsync(id);

            if (poll == null)
            {
                return NotFound(new { error = "Encuesta no encontrada" });
            }

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
        public async Task<IActionResult> GetAllPollsWithResults()
        {
            var polls = await _getAllPollsWithResults.ExecuteAsync();

            if (polls == null || !polls.Any())
            {
                return NotFound(new { error = "No se encontraron encuestas." });
            }

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

        [HttpGet("polls/active-count")]
        public async Task<IActionResult> GetActivePollCount(
    [FromQuery] int consortiumId,
    [FromQuery] DateTime? dateTime = null)
        {
            var count = await _getActivePollCount.ExecuteAsync(consortiumId, dateTime);
            return Ok(new
            {
                activePolls = count,
                checkedAt = (dateTime ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }



}
