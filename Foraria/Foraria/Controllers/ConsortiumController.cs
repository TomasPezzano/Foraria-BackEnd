using Foraria.Application.Services;
using Foraria.Domain.Repository;
using Foraria.DTOs;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Foraria.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsortiumController : ControllerBase
{
    private readonly ISelectConsortium _selectConsortium;
    private readonly IAssignConsortiumToAdmin _assignConsortiumToAdmin;
    private readonly IGetUserConsortiums _getUserConsortiums;
    private readonly IPermissionService _permissionService;
    private readonly IUserRepository _userRepository;
    private readonly IConsortiumRepository _consortiumRepository;

    public ConsortiumController(
        ISelectConsortium selectConsortium,
        IAssignConsortiumToAdmin assignConsortiumToAdmin,
        IGetUserConsortiums getUserConsortiums,
        IPermissionService permissionService,
        IUserRepository userRepository,
        IConsortiumRepository consortiumRepository)
    {
        _selectConsortium = selectConsortium;
        _assignConsortiumToAdmin = assignConsortiumToAdmin;
        _getUserConsortiums = getUserConsortiums;
        _permissionService = permissionService;
        _userRepository = userRepository;
        _consortiumRepository = consortiumRepository;
    }


    [HttpPost("select")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Selecciona el consorcio activo",
        Description = "Permite a administradores y consejo cambiar el consorcio con el que están trabajando. " +
                      "Esto establece el contexto para todas las operaciones subsecuentes."
    )]
    [ProducesResponseType(typeof(SelectConsortiumResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SelectConsortium([FromBody] SelectConsortiumRequestDto request)
    {
        await _permissionService.EnsurePermissionAsync(User, "Consortium.Select");

        if (!ModelState.IsValid)
            throw new DomainValidationException("Datos inválidos.");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedException("Token inválido.");

        var result = await _selectConsortium.Execute(userId, request.ConsortiumId);

        var response = new SelectConsortiumResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            ConsortiumInfo = new ConsortiumInfoDto
            {
                Id = result.Consortium!.Id,
                Name = result.Consortium.Name,
                Description = result.Consortium.Description
            }
        };

        return Ok(response);
    }


    [HttpGet("available")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Obtiene los consorcios disponibles para el usuario",
        Description = "Retorna la lista de consorcios a los que el usuario actual tiene acceso. " +
                      "Para administradores: consorcios donde es administrador. " +
                      "Para propietarios/inquilinos/consejo: consorcios de sus residencias."
    )]
    [ProducesResponseType(typeof(List<ConsortiumInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableConsortiums()
    {
        await _permissionService.EnsurePermissionAsync(User, "Consortium.ViewAvailable");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedException("Token inválido.");

        var consortiums = await _getUserConsortiums.Execute(userId);

        var response = consortiums.Select(c => new ConsortiumInfoDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();

        return Ok(response);
    }

    [HttpPost("assign")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
    Summary = "Asigna un consorcio a un administrador",
    Description = "Permite agregar un consorcio adicional a un administrador existente. " +
                  "El administrador puede gestionar múltiples consorcios. " +
                  "Validaciones: El usuario debe ser Administrador y el consorcio no puede tener otro admin asignado.")]
    [ProducesResponseType(typeof(AssignConsortiumResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignConsortiumToAdmin([FromBody] AssignConsortiumRequestDto request)
    {
        await _permissionService.EnsurePermissionAsync(User, "Consortium.AssignToAdmin");

        if (!ModelState.IsValid)
            throw new DomainValidationException("Datos inválidos.");

        var result = await _assignConsortiumToAdmin.Execute(
            request.AdministratorId,
            request.ConsortiumId);

        var response = new AssignConsortiumResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            AdminInfo = new AdminConsortiumsDto
            {
                AdministratorId = result.Admin.Id,
                AdministratorName = $"{result.Admin.Name} {result.Admin.LastName}",
                AdministratorEmail = result.Admin.Mail,
                AssignedConsortiums = result.Consortiums.Select(c => new ConsortiumInfoDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
            }
        };

        return Ok(response);
    }

    [HttpGet("admin/{administratorId}")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
    Summary = "Obtiene los consorcios de un administrador",
    Description = "Retorna la lista completa de consorcios que gestiona un administrador específico.")]
    [ProducesResponseType(typeof(AdminConsortiumsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdminConsortiums(int administratorId)
    {
        await _permissionService.EnsurePermissionAsync(User, "Consortium.ViewAdminConsortiums");

        var admin = await _userRepository.GetByIdWithoutFilters(administratorId);

        if (admin == null)
            throw new NotFoundException($"Administrador con ID {administratorId} no encontrado.");

        if (admin.Role.Description != "Administrador")
            throw new BusinessException("El usuario especificado no es un Administrador.");

        var consortiums = await _consortiumRepository.GetConsortiumsByAdministrator(administratorId);

        var response = new AdminConsortiumsDto
        {
            AdministratorId = admin.Id,
            AdministratorName = $"{admin.Name} {admin.LastName}",
            AdministratorEmail = admin.Mail,
            AssignedConsortiums = consortiums.Select(c => new ConsortiumInfoDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList()
        };

        return Ok(response);
    }
}
