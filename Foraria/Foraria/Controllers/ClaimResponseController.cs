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
    public async Task<IActionResult> Add([FromBody] ClaimResponseDto claimResponseDto)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos de la respuesta no son válidos.");

        var user = await _getUserById.Execute(claimResponseDto.User_id);
        if (user == null)
            throw new NotFoundException($"No se encontró el usuario con ID {claimResponseDto.User_id}.");

        var claim = await _getClaimById.Execute(claimResponseDto.Claim_id);
        if (claim == null)
            throw new NotFoundException($"No se encontró el reclamo con ID {claimResponseDto.Claim_id}.");

        var sector = await _getResponsibleSectorById.Execute(claimResponseDto.ResponsibleSector_id);
        if (sector == null)
            throw new NotFoundException($"No se encontró el sector responsable con ID {claimResponseDto.ResponsibleSector_id}.");

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
        if (responseResult == null)
            throw new BusinessException("No se pudo registrar la respuesta del reclamo.");

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
}
