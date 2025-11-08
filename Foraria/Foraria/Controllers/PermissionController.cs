using Foraria.Application.UseCase;
using Foraria.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Policy = "Owner")]
    public async Task<IActionResult> TransferPermission([FromBody] TransferPermissionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(ownerIdClaim, out int ownerId))
        {
            return Unauthorized(new { message = "Token inválido" });
        }

        await _transferPermission.Execute(ownerId, request.TenantId);

        return Ok(new
        {
            message = "Permisos transferidos exitosamente. El inquilino debe iniciar sesión nuevamente para obtener los nuevos permisos.",
            tenantId = request.TenantId
        });
    }


    [HttpPost("revoke")]
    [Authorize(Policy = "Owner")]
    public async Task<IActionResult> RevokePermission([FromBody] TransferPermissionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(ownerIdClaim, out int ownerId))
        {
            return Unauthorized(new { message = "Token inválido" });
        }

        var tenantId = request.TenantId;

        await _revokePermission.Execute(ownerId, tenantId);


        return Ok(new
        {
            message = "Permisos revocados exitosamente. El inquilino debe iniciar sesión nuevamente.",
            tenantId = request.TenantId
        });
    }
}