using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
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
        public async Task<IActionResult> Create([FromBody] CreateForumRequest request)
        {
            var createdForum = await _createForum.Execute(request);
            return CreatedAtAction(nameof(GetById), new { id = createdForum.Id }, createdForum);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var forum = await _repository.GetById(id);
            if (forum == null) return NotFound();

            var response = new ForumResponse
            {
                Id = forum.Id,
                Category = forum.Category
            };

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var forums = await _repository.GetAll();

            var response = forums.Select(f => new ForumResponse
            {
                Id = f.Id,
                Category = f.Category
            });

            return Ok(response);
        }
    }
}