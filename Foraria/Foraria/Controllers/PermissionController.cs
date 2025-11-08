using Foraria.Application.UseCase;
using Foraria.DTOs;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Foraria.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionController : ControllerBase
{
    private readonly ITransferPermission _transferPermission;
    private readonly IRevokePermission _revokePermission;

    public PermissionController(ITransferPermission transferPermission, IRevokePermission revokePermission)
    {
        _transferPermission = transferPermission;
        _revokePermission = revokePermission;
    }

    [HttpPost("transfer")]
    //[Authorize(Policy = "Owner")]
    [SwaggerOperation(
        Summary = "Transfiere los permisos del propietario a un inquilino.",
        Description = "Permite al propietario transferir sus permisos de acceso al inquilino indicado. El inquilino deberá iniciar sesión nuevamente para obtenerlos."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TransferPermission([FromBody] TransferPermissionRequestDto request)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos de la solicitud no son válidos.");

        var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(ownerIdClaim, out int ownerId))
            throw new UnauthorizedException("Token inválido o usuario no autenticado.");

        await _transferPermission.Execute(ownerId, request.TenantId);

        return Ok(new
        {
            message = "Permisos transferidos exitosamente. El inquilino debe iniciar sesión nuevamente para obtener los nuevos permisos.",
            tenantId = request.TenantId
        });
    }

    [HttpPost("revoke")]
    //[Authorize(Policy = "Owner")]
    [SwaggerOperation(
        Summary = "Revoca los permisos del inquilino.",
        Description = "Permite al propietario revocar los permisos que había transferido al inquilino, invalidando su acceso actual."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokePermission([FromBody] TransferPermissionRequestDto request)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos de la solicitud no son válidos.");

        var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(ownerIdClaim, out int ownerId))
            throw new UnauthorizedException("Token inválido o usuario no autenticado.");

        var tenantId = request.TenantId;

        await _revokePermission.Execute(ownerId, tenantId);

        return Ok(new
        {
            message = "Permisos revocados exitosamente. El inquilino debe iniciar sesión nuevamente.",
            tenantId = request.TenantId
        });
    }
}