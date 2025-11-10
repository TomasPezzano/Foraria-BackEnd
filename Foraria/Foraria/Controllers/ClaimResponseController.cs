using Foraria.Application.UseCase;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Crea una respuesta para un reclamo existente.",
        Description = "Permite al consorcio o administrador registrar una respuesta a un reclamo, asociándola con un usuario y un sector responsable."
    )]
    [ProducesResponseType(typeof(ClaimResponseResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ClaimResponseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _getUserById.Execute(dto.User_id);
            if (user == null)
                return NotFound(new { error = "Usuario no encontrado" });

            var claim = await _getClaimById.Execute(dto.Claim_id);
            if (claim == null)
                return NotFound(new { error = "Reclamo no encontrado" });

            var sector = await _getResponsibleSectorById.Execute(dto.ResponsibleSector_id);
            if (sector == null)
                return NotFound(new { error = "Sector responsable no encontrado" });

            var claimResponse = new ClaimResponse
            {
                Description = dto.Description,
                ResponseDate = dto.ResponseDate,
                ResponsibleSector_id = dto.ResponsibleSector_id,
                User = user,
                Claim = claim
            };

            var result = await _createClaimResponse.Execute(claimResponse);

            // Enriquecemos el resultado con info adicional para devolver al front
            var response = new
            {
                result.Id,
                result.Description,
                result.ResponseDate,
                result.User_id,
                UserName = user.Name,
                result.Claim_id,
                ClaimTitle = claim.Title,
                result.ResponsibleSector_id,
                ResponsibleSectorName = sector.Name
            };

            return CreatedAtAction(nameof(Add), new { id = result.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ForariaDomain.Exceptions.NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForariaDomain.Exceptions.DomainValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Ocurrió un error interno",
                details = ex.Message
            });
        }
    }

}
