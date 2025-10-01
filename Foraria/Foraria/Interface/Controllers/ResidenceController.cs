using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
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
    public async Task<IActionResult> CreateResidence([FromBody] ResidenceRequestDto residence)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _createResidenceUseCase.Create(residence);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    [HttpGet("getById/{id}")]
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
    public async Task<IActionResult> GetAllResidences()
    {
        var residences = await _createResidenceUseCase.GetAllResidences();
        return Ok(residences);
    }
}