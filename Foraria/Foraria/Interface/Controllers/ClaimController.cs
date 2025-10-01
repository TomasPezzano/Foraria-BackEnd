using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClaimController : ControllerBase
{

    public readonly CreateClaim CreateClaim;
    public readonly GetClaims GetClaims;
    public ClaimController(CreateClaim CreateClaim, GetClaims GetClaims)
    {
        this.CreateClaim = CreateClaim;
        this.GetClaims = GetClaims;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        List<Claim> claims = GetClaims.execute();
        return Ok(claims);
    }

    [HttpPost]
    public IActionResult Add([FromBody] ClaimDto claimDto)
    {
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        CreateClaim.Execute(claimDto);

        return Ok("Claim creado correctamente");
    }


}
