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
public class ClaimController : ControllerBase
{
    private readonly ICreateClaim _createClaim;
    private readonly IGetClaims _getClaims;
    private readonly IRejectClaim _rejectClaim;
    private readonly IFileProcessor _fileProcessor;

    public ClaimController(
        ICreateClaim createClaim,
        IGetClaims getClaims,
        IRejectClaim rejectClaim,
        IFileProcessor fileProcessor)
    {
        _createClaim = createClaim;
        _getClaims = getClaims;
        _rejectClaim = rejectClaim;
        _fileProcessor = fileProcessor;
    }


    [HttpGet]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Obtiene la lista de reclamos registrados.",
        Description = "Devuelve una lista con todos los reclamos existentes junto con su información básica, usuario asociado y respuesta (si existe)."
    )]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var claims = await _getClaims.Execute();

        if (claims == null || !claims.Any())
            throw new NotFoundException("No se encontraron reclamos registrados.");

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
                Residences = c.User.Residences?
                    .Select(r => new ResidenceDto
                    {
                        Id = r.Id,
                        ConsortiumId = r.ConsortiumId
                    }).ToList()
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


    [HttpPost]
    [Authorize(Policy = "OwnerAndTenant")]
    [SwaggerOperation(
        Summary = "Crea un nuevo reclamo.",
        Description = "Permite a un propietario o inquilino crear un nuevo reclamo, opcionalmente con archivo adjunto en formato Base64."
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromBody] ClaimDto claimDto)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos del reclamo no son válidos.");

        string? filePath = null;

        if (!string.IsNullOrEmpty(claimDto.Archive))
        {
            try
            {
                filePath = await _fileProcessor.SaveBase64FileAsync(claimDto.Archive, "claims");
            }
            catch (FormatException)
            {
                throw new DomainValidationException("El formato del archivo Base64 no es válido.");
            }
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

        if (createdClaim == null)
            throw new BusinessException("No se pudo crear el reclamo.");

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


    [HttpPut("reject/{id}")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Rechaza un reclamo específico.",
        Description = "Permite a un administrador o consorcio marcar un reclamo como rechazado según su ID."
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RejectClaimById(int id)
    {
        if (id <= 0)
            throw new DomainValidationException("El ID del reclamo no es válido.");

        await _rejectClaim.Execute(id);

        return Ok(new { message = $"El reclamo con ID {id} fue rechazado correctamente." });
    }
}
