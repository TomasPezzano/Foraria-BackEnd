using Foraria.Application.UseCase;
using ForariaDomain;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumController : ControllerBase
    {
        private readonly CreateForum _createForum;

        public ForumController(CreateForum createForum)
        {
            _createForum = createForum;
        }

        [HttpPost]
        public async Task<ActionResult<Forum>> Create([FromBody] Forum forum)
        {
            var created = await _createForum.Execute(forum);
            return CreatedAtAction(nameof(Create), new { id = created.Id }, created);
        }
    }
}