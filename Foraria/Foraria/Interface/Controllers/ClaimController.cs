using System.Threading.Tasks;
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

    public readonly CreateClaim _createClaim;
    public readonly GetClaims _getClaims;
    public readonly RejectClaim _rejectClaim;
    public ClaimController(CreateClaim CreateClaim, GetClaims GetClaims, RejectClaim rejectClaim)
    {
        _createClaim = CreateClaim;
        _getClaims = GetClaims;
        _rejectClaim = rejectClaim;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<Claim> claims = await _getClaims.execute();
        var result = claims.Select(c => new
        {
            claim = new ClaimDto
            {
                Title = c.Title,
                Description = c.Description,
                Priority = c.Priority,
                Category = c.Category,
                Archive = c.Archive,
                User_id = c.User_id
            },

            claimResponse = c.ClaimResponse != null ? new ClaimResponseDto
            {
                Description = c.ClaimResponse.Description,
                ResponseDate = c.ClaimResponse.ResponseDate,
                User_id = c.ClaimResponse.User.Id,
                Claim_id = c.ClaimResponse.Claim.Id,
                ResponsibleSector_id = c.ClaimResponse.ResponsibleSector_id
            } : null
        }).ToList();

        return Ok(result);
    }

    [HttpPost]
     public async Task<IActionResult> Add([FromBody] ClaimDto claimDto)
    {
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var claim = await _createClaim.Execute(claimDto);

        var claimResult = new ClaimDto
        {
            Title = claim.Title,
            Description = claim.Description,
            Priority = claim.Priority,
            Category = claim.Category,
            Archive = claim.Archive,
            User_id = claim.User_id
        };

        return CreatedAtAction(nameof(GetAll), new { id = claim.Id }, claimResult);
    }

    [HttpPut("reject/{id}")]
    public async Task<IActionResult> RejectClaimById(int id)
    {
        try
        {
            await _rejectClaim.Execute(id);
            return Ok(new { message = $"El reclamo con ID {id} fue rechazado correctamente" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ocurrió un error interno", details = ex.Message });
        }
    }


}
