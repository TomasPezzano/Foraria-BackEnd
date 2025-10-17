using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClaimResponseController : ControllerBase
{
    private readonly ICreateClaimResponse _createClaimResponse;
    private readonly IUserRepository _userRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IResponsibleSectorRepository _responsibleSectorRepository;

    public ClaimResponseController(
        ICreateClaimResponse createClaimResponse,
        IUserRepository userRepository,
        IClaimRepository claimRepository,
        IResponsibleSectorRepository responsibleSectorRepository)
    {
        _createClaimResponse = createClaimResponse;
        _userRepository = userRepository;
        _claimRepository = claimRepository;
        _responsibleSectorRepository = responsibleSectorRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ClaimResponseDto claimResponseDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userRepository.GetById(claimResponseDto.User_id);
            if (user == null)
                return BadRequest(new { error = "Usuario no encontrado" });

            var claim = await _claimRepository.GetById(claimResponseDto.Claim_id);
            if (claim == null)
                return BadRequest(new { error = "Reclamo no encontrado" });

            var sector = await _responsibleSectorRepository.GetById(claimResponseDto.ResponsibleSector_id);
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
