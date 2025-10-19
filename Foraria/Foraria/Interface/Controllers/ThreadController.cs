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
        private readonly GetAllThreads _getAllThreads;
        private readonly DeleteThread _deleteThread;
        private readonly UpdateThread _updateThread;
        private readonly GetThreadWithMessages _getThreadWithMessages;
        private readonly CloseThread _closeThread;
        private readonly GetThreadCommentCount _getThreadCommentCount;

        public ThreadController(CreateThread createThread, GetThreadById getThreadById, GetAllThreads getAllThreads, DeleteThread deleteThread, UpdateThread updateThread, GetThreadWithMessages getThreadWithMessages, CloseThread closeThread, GetThreadCommentCount getThreadCommentCount)
        {
            _createThread = createThread;
            _getThreadById = getThreadById;
            _getAllThreads = getAllThreads;
            _deleteThread = deleteThread;
            _updateThread = updateThread;
            _getThreadWithMessages = getThreadWithMessages;
            _closeThread = closeThread;
            _getThreadCommentCount = getThreadCommentCount;
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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? forumId)
        {
            var threads = await _getAllThreads.ExecuteAsync(forumId);
            return Ok(threads);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _deleteThread.ExecuteAsync(id);
                return NoContent(); // 204 OK
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateThreadRequest request)
        {
            try
            {
                var updated = await _updateThread.ExecuteAsync(id, request);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetThreadWithMessages(int id)
        {
            var thread = await _getThreadWithMessages.ExecuteAsync(id);
            if (thread == null)
                return NotFound(new { message = $"No se encontró el thread con ID {id}" });

            return Ok(thread);
        }

        [HttpPatch("{id}/close")]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                var closed = await _closeThread.ExecuteAsync(id);
                return Ok(closed);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{threadId}/comment-count")]
        public async Task<IActionResult> GetCommentCount(int threadId)
        {
            try
            {
                var count = await _getThreadCommentCount.Execute(threadId);
                return Ok(new { ThreadId = threadId, CommentCount = count });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


    }
}
