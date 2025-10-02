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

    public readonly CreateClaimResponse CreateClaimResponse;
    public ClaimResponseController(CreateClaimResponse CreateClaimResponse)
    {
        this.CreateClaimResponse = CreateClaimResponse;
    }

    [HttpPost]
    public IActionResult Add([FromBody] ClaimResponseDto claimResponseDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var responseResult = CreateClaimResponse.Execute(claimResponseDto);

        return Ok(responseResult);
    }

    
}
