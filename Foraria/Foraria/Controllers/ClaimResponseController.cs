using Foraria.Application.UseCase;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Foraria.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class ClaimResponseController : ControllerBase
{
    private readonly ICreateClaimResponse _createClaimResponse;
    private readonly IGetUserById _getUserById;
    private readonly IGetClaimById _getClaimById;
    private readonly IGetResponsibleSectorById _getResponsibleSectorById;

    public ClaimResponseController(
        ICreateClaimResponse createClaimResponse,
        IGetUserById getUserById,
        IGetClaimById getClaimById,
        IGetResponsibleSectorById getResponsibleSectorById)
    {
        _createClaimResponse = createClaimResponse;
        _getUserById = getUserById;
        _getClaimById = getClaimById;
        _getResponsibleSectorById = getResponsibleSectorById;
    }


    [HttpPost]
    [ProducesResponseType(typeof(ClaimResponseResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Policy = "ConsortiumAndAdmin")]

    public async Task<IActionResult> Add([FromBody] ClaimResponseDto claimResponseDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _getUserById.Execute(claimResponseDto.User_id);
            if (user == null)
                return NotFound(new { error = "Usuario no encontrado" });

            var claim = await _getClaimById.Execute(claimResponseDto.Claim_id);
            if (claim == null)
                return NotFound(new { error = "Reclamo no encontrado" });

            var sector = await _getResponsibleSectorById.Execute(claimResponseDto.ResponsibleSector_id);
            if (sector == null)
                return NotFound(new { error = "Sector responsable no encontrado" });

            var claimResponse = new ClaimResponse
            {
                Description = claimResponseDto.Description,
                ResponseDate = claimResponseDto.ResponseDate,
                User = user,
                Claim = claim,
                ResponsibleSector_id = sector.Id,
                ResponsibleSector = sector
            };

            var responseResult = await _createClaimResponse.Execute(claimResponse);

            var responseDto = new ClaimResponseResultDto
            {
                Id = claimResponse.Id,
                Description = claimResponse.Description,
                ResponseDate = claimResponse.ResponseDate,
                User_id = claimResponse.User?.Id ?? claimResponseDto.User_id,
                UserName = claimResponse.User?.Name,
                Claim_id = claimResponse.Claim?.Id ?? claimResponseDto.Claim_id,
                ClaimTitle = claimResponse.Claim?.Title,
                ResponsibleSector_id = claimResponse.ResponsibleSector_id,
                ResponsibleSectorName = claimResponse.ResponsibleSector?.Name
            };

            return CreatedAtAction(nameof(Add), new { id = responseDto.Id }, responseDto);
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

