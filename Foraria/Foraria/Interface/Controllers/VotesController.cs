using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotesController : ControllerBase
    {
        private readonly CreateVote _createVoteUseCase;

        public VotesController(CreateVote createVoteUseCase)
        {
            _createVoteUseCase = createVoteUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> PostVote([FromBody] VoteDto request)
        {
            try
            {
                await _createVoteUseCase.ExecuteAsync(request.User_Id, request.Poll_Id, request.PollOption_Id);
                return Ok(new { message = "Voto registrado correctamente" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message }); 
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocurrió un error inesperado", detail = ex.Message });
            }
        }
    }

}
