using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumController : ControllerBase
    {
        private readonly CreateForum _createForum;
        private readonly GetForumById _getForumById;
        private readonly GetAllForums _getAllForums;

        public ForumController(CreateForum createForum, GetForumById getForumById, GetAllForums getAllForums)
        {
            _createForum = createForum;
            _getForumById = getForumById;
            _getAllForums = getAllForums;
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
            var response = await _getForumById.Execute(id);
            if (response == null) return NotFound();
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var responses = await _getAllForums.Execute();
            return Ok(responses);
        }
    }
}
