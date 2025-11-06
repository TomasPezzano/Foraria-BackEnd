using Foraria.Application.UseCase;
using Foraria.DTOs;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> CreateResidence([FromBody] ResidenceRequestDto residenceDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _createResidenceUseCase.Create(
            residenceDto.ConsortiumId,
            residenceDto.Number,
            residenceDto.Floor,
            residenceDto.Tower
        );

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        // Mapeo de entidad a DTO
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
    public async Task<IActionResult> GetResidenceById(int id)
    {
        var result = await _getResidenceById.Execute(id);

        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }

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
    public async Task<IActionResult> GetAllResidencesByConsortium(int idConsortium)
    {
        var result = await _getAllResidencesByConsortium.ExecuteAsync(idConsortium);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

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