using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThreadController : ControllerBase
    {
        private readonly CreateThread _createThread;
        private readonly GetThreadById _getThreadById;

        public ThreadController(CreateThread createThread, GetThreadById getThreadById)
        {
            _createThread = createThread;
            _getThreadById = getThreadById;
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
            var response = await _getThreadById.Execute(id);
            if (response == null) return NotFound();
            return Ok(response);
        }
    }
}
