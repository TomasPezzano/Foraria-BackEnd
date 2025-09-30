using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using Thread = ForariaDomain.Thread; //la entidad Thread choca con System.Threading.Thread 

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

        //nuevo hilo

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Thread thread)
        {
            var createdThread = await _createThread.Execute(thread);
            return CreatedAtAction(nameof(GetById), new { id = createdThread.Id }, createdThread);
        }

        //get hilo por id

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var thread = await _repository.GetById(id);
            if (thread == null) return NotFound();
            return Ok(thread);
        }
    }
}

