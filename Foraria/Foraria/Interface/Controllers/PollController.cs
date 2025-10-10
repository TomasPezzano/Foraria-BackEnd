using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/polls")]
    public class PollController : Controller
    {
        private readonly CreatePoll _poll;
        private readonly GetPolls _polls;
        private readonly NotarizePoll _notarizePoll; 
        private readonly GetPollById _getPollById;
        public PollController(CreatePoll poll, GetPolls polls, GetPollById getPollById, NotarizePoll notarizePoll)
        {
            _poll = poll;
            _polls = polls;
            _getPollById = getPollById;
            _notarizePoll = notarizePoll;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PollDto request)
        {
            try
            {
                var result = await _poll.ExecuteAsync(request);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _polls.ExecuteAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var poll = await _getPollById.ExecuteAsync(id);
            if (poll == null)
                return NotFound();

            return Ok(poll);
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
