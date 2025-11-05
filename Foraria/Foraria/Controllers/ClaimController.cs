using Foraria.Application.UseCase;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class ClaimController : ControllerBase
{
    private readonly ICreateClaim _createClaim;
    private readonly IGetClaims _getClaims;
    private readonly IRejectClaim _rejectClaim;
    private readonly IFileProcessor _fileProcessor;

    public ClaimController(ICreateClaim CreateClaim, IGetClaims GetClaims, IRejectClaim rejectClaim, IFileProcessor fileProcessor)
    {
        _createClaim = CreateClaim;
        _getClaims = GetClaims;
        _rejectClaim = rejectClaim;
        _fileProcessor = fileProcessor;
    }


    [HttpGet]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Policy = "All")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            List<Claim> claims = await _getClaims.Execute();

            var result = claims.Select(c => new
            {
                claim = new ClaimDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    State = c.State,
                    Priority = c.Priority,
                    Category = c.Category,
                    Archive = c.Archive,
                    User_id = c.User_id,
                    CreatedAt = c.CreatedAt
                },

                user = c.User != null ? new UserDto
                {
                    Id = c.User.Id,
                    FirstName = c.User.Name,
                    LastName = c.User.LastName,
                    Residences = (List<ResidenceDto>)c.User.Residences
                } : null,

                claimResponse = c.ClaimResponse != null ? new ClaimResponseDto
                {
                    Description = c.ClaimResponse.Description,
                    ResponseDate = c.ClaimResponse.ResponseDate,
                    User_id = c.ClaimResponse.User.Id,
                    Claim_id = c.ClaimResponse.Claim.Id
                } : null,

                responsibleSectorName = c.ClaimResponse?.ResponsibleSector?.Name
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener los reclamos", details = ex.Message });
        }
    }


    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Policy = "OwnerAndTenant")]

    public async Task<IActionResult> Add([FromBody] ClaimDto claimDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? filePath = null;
            if (!string.IsNullOrEmpty(claimDto.Archive))
            {
                filePath = await _fileProcessor.SaveBase64FileAsync(claimDto.Archive, "claims");
            }

            var claim = new Claim
            {
                Title = claimDto.Title,
                Description = claimDto.Description,
                Priority = claimDto.Priority,
                Category = claimDto.Category,
                Archive = filePath,
                User_id = claimDto.User_id,
                ResidenceId = claimDto.ResidenceId,
                CreatedAt = DateTime.UtcNow,
                State = "Nuevo"
            };
            var createdClaim = await _createClaim.Execute(claim);

            var response = new
            {
                createdClaim.Id,
                createdClaim.Title,
                createdClaim.Description,
                createdClaim.Priority,
                createdClaim.Category,
                ArchiveUrl = filePath
            };

            return CreatedAtAction(nameof(GetAll), new { id = createdClaim.Id }, response);
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "El formato del archivo Base64 no es válido." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }


    [HttpPut("reject/{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Policy = "ConsortiumAndAdmin")]

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
