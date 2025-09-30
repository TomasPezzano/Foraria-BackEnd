using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumsController : ControllerBase
    {
        private readonly CreateForum _createForum;
        private readonly IForumRepository _repository;

        public ForumsController(CreateForum createForum, IForumRepository repository)
        {
            _createForum = createForum;
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Forum forum)
        {
            var createdForum = await _createForum.Execute(forum);
            return CreatedAtAction(nameof(GetById), new { id = createdForum.Id }, createdForum);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var forum = await _repository.GetById(id);
            if (forum == null) return NotFound();
            return Ok(forum);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var forums = await _repository.GetAll();
            return Ok(forums);
        }
    }
}