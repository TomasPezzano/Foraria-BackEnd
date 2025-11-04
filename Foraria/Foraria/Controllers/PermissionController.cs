using Foraria.Application.UseCase;
using Foraria.Contracts.DTOs;
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
    //[Authorize(Policy = "Owner")]
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

        var result = await _transferPermission.Execute(ownerId, request.TenantId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }


    [HttpPost("revoke")]
    //[Authorize(Policy = "Owner")]
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

        var result = await _revokePermission.Execute(ownerId, request.TenantId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}