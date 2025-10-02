using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClaimResponseController : ControllerBase
{

    public readonly CreateClaimResponse _createClaimResponse;
    public ClaimResponseController(CreateClaimResponse CreateClaimResponse)
    {
        _createClaimResponse = CreateClaimResponse;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ClaimResponseDto claimResponseDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var responseResult = await _createClaimResponse.Execute(claimResponseDto);

            return Ok(responseResult);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ocurrió un error interno", details = ex.Message });
        }
    }
}

    