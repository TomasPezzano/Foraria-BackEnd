using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")]
[ApiController]
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
    public async Task<IActionResult> Add([FromBody] ClaimResponseDto claimResponseDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _getUserById.Execute(claimResponseDto.User_id);
            if (user == null)
                return BadRequest(new { error = "Usuario no encontrado" });

            var claim = await _getClaimById.Execute(claimResponseDto.Claim_id);
            if (claim == null)
                return BadRequest(new { error = "Reclamo no encontrado" });

            var sector = await _getResponsibleSectorById.Execute(claimResponseDto.ResponsibleSector_id);
            if (sector == null)
                return BadRequest(new { error = "Sector responsable no encontrado" });

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
