using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;
using Thread = ForariaDomain.Thread;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThreadsController : ControllerBase
    {
        private readonly CreateThread _createThread;
        private readonly IThreadRepository _repository;

        public ThreadsController(CreateThread createThread, IThreadRepository repository)
        {
            _createThread = createThread;
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateThreadRequest request)
        {
            var createdThread = await _createThread.Execute(request);
            return CreatedAtAction(nameof(GetById), new { id = createdThread.Id }, createdThread);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var thread = await _repository.GetById(id);
            if (thread == null) return NotFound();

            var response = new ThreadResponse
            {
                Id = thread.Id,
                Theme = thread.Theme,
                Description = thread.Description,
                CreatedAt = thread.CreatedAt,
                State = thread.State,
                Forum_id = thread.Forum_id,
                User_id = thread.User_id
            };

            return Ok(response);
        }
    }
}
