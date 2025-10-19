using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using ForariaDomain.Application.UseCase;
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
        private readonly GetForumWithThreads _getForumWithThreads;
        private readonly DeleteForum _deleteForum;

        public ForumController(CreateForum createForum, GetForumById getForumById, GetAllForums getAllForums, GetForumWithThreads getForumWithThreads, DeleteForum deleteForum)
        {
            _createForum = createForum;
            _getForumById = getForumById;
            _getAllForums = getAllForums;
            _getForumWithThreads = getForumWithThreads;
            _deleteForum = deleteForum;
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

        [HttpGet("{id}/threads")]
        public async Task<IActionResult> GetForumWithThreads(int id)
        {
            var forum = await _getForumWithThreads.Execute(id);
            if (forum == null) return NotFound($"No se encontró el foro con ID {id}");

            return Ok(forum);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _deleteForum.Execute(id);
                return NoContent(); // 204 OK, sin body
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
