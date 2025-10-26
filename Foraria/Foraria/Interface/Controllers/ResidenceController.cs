using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResidenceController : ControllerBase
{
    private readonly ICreateResidence _createResidenceUseCase;

    public ResidenceController(ICreateResidence createResidenceUseCase)
    {
        _createResidenceUseCase = createResidenceUseCase;
    }

    [HttpPost("create")]
    [Authorize(Policy = "OnlyConsortium")]

    public async Task<IActionResult> CreateResidence([FromBody] ResidenceRequestDto residenceDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }


        var residence = new Residence
        {
            Number = residenceDto.Number,
            Floor = residenceDto.Floor,
            Tower = residenceDto.Tower,
            ConsortiumId = residenceDto.ConsortiumId
        };

        var result = await _createResidenceUseCase.Create(residence);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    [HttpGet("getById/{id}")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    public async Task<IActionResult> GetResidenceById(int id)
    {
        var result = await _createResidenceUseCase.GetResidenceById(id);

        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }

        return Ok(result);
    }

    [HttpGet("getAll")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    public async Task<IActionResult> GetAllResidences()
    {
        var residences = await _createResidenceUseCase.GetAllResidences();
        return Ok(residences);
    }
}