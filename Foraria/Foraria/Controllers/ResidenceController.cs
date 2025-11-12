using Foraria.DTOs;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResidenceController : ControllerBase
{
    private readonly ICreateResidence _createResidenceUseCase;
    private readonly IGetAllResidencesByConsortium _getAllResidencesByConsortium;
    private readonly IGetResidenceById _getResidenceById;

    public ResidenceController(
        ICreateResidence createResidenceUseCase,
        IGetAllResidencesByConsortium getAllResidencesByConsortium,
        IGetResidenceById getResidenceById)
    {
        _createResidenceUseCase = createResidenceUseCase;
        _getAllResidencesByConsortium = getAllResidencesByConsortium;
        _getResidenceById = getResidenceById;
    }

    [HttpPost("create")]
    //[Authorize(Policy = "OnlyConsortium")]
    [SwaggerOperation(
        Summary = "Crea una nueva residencia.",
        Description = "Permite registrar una residencia dentro de un consorcio existente."
    )]
    [ProducesResponseType(typeof(ResidenceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateResidence([FromBody] ResidenceRequestDto residenceDto)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos de la residencia no son válidos.");

        if (residenceDto.ConsortiumId <= 0)
            throw new DomainValidationException("Debe especificar un ID de consorcio válido.");

        var result = await _createResidenceUseCase.Create(
            residenceDto.ConsortiumId,
            residenceDto.Number,
            residenceDto.Floor,
            residenceDto.Tower
        );

        if (!result.Success)
            throw new BusinessException(result.Message);

        var responseDto = new ResidenceResponseDto
        {
            Id = result.Residence.Id,
            Number = result.Residence.Number,
            Floor = result.Residence.Floor,
            Tower = result.Residence.Tower,
            Success = true,
            Message = result.Message
        };

        return Ok(responseDto);
    }

    [HttpGet("getById/{id}")]
    //[Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Obtiene una residencia por su ID.",
        Description = "Devuelve los detalles de una residencia específica si existe."
    )]
    [ProducesResponseType(typeof(ResidenceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetResidenceById(int id)
    {
        if (id <= 0)
            throw new DomainValidationException("El ID de la residencia debe ser válido.");

        var result = await _getResidenceById.Execute(id);

        if (!result.Success)
            throw new NotFoundException(result.Message);

        var responseDto = new ResidenceResponseDto
        {
            Id = result.Residence.Id,
            Number = result.Residence.Number,
            Floor = result.Residence.Floor,
            Tower = result.Residence.Tower,
            Success = true,
            Message = result.Message
        };

        return Ok(responseDto);
    }

    [HttpGet("getAllResidencesByConsortium")]
    //[Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Obtiene todas las residencias de un consorcio.",
        Description = "Devuelve la lista completa de residencias pertenecientes al consorcio indicado."
    )]
    [ProducesResponseType(typeof(IEnumerable<ResidenceResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllResidencesByConsortium(int idConsortium)
    {
        if (idConsortium <= 0)
            throw new DomainValidationException("Debe especificar un ID de consorcio válido.");

        var result = await _getAllResidencesByConsortium.ExecuteAsync(idConsortium);

        if (!result.Success)
            throw new NotFoundException(result.Message);

        var residencesDto = result.Residences.Select(r => new ResidenceResponseDto
        {
            Id = r.Id,
            Number = r.Number,
            Floor = r.Floor,
            Tower = r.Tower
        }).ToList();

        return Ok(new
        {
            success = true,
            message = result.Message,
            residences = residencesDto
        });
    }
}
