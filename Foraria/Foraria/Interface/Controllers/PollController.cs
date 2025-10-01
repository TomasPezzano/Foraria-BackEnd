using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/polls")]
    public class PollController : Controller
    {
        private readonly CreatePoll _poll;
        private readonly GetPolls _polls;
        public PollController(CreatePoll poll, GetPolls polls){
            _poll = poll;
            _polls = polls;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PollDto request)
        {
            var result = await _poll.ExecuteAsync(request);
            return Ok(result); 
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _polls.ExecuteAsync();
            return Ok(result);
        }
    }



}
