using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReactionsController : ControllerBase
    {
        private readonly ToggleReaction _toggleReaction;
        private readonly IReactionRepository _repository;

        public ReactionsController(ToggleReaction toggleReaction, IReactionRepository repository)
        {
            _toggleReaction = toggleReaction;
            _repository = repository;
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> Toggle([FromBody] ReactionRequest request)
        {
            var result = await _toggleReaction.Execute(
                request.User_id,
                request.Message_id,
                request.Thread_id,
                request.ReactionType
            );

            return Ok(new { success = result });
        }

        [HttpGet("message/{messageId}")]
        public async Task<IActionResult> GetMessageReactions(int messageId)
        {
            var total = await _repository.CountByMessage(messageId);
            return Ok(new ReactionResponse { Total = total });
        }

        [HttpGet("thread/{threadId}")]
        public async Task<IActionResult> GetThreadReactions(int threadId)
        {
            var total = await _repository.CountByThread(threadId);
            return Ok(new ReactionResponse { Total = total });
        }
    }
}