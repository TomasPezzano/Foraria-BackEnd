using Foraria.Contracts.DTOs;
using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase;

public interface IRevokePermission
{
    Task<TransferPermissionResponseDto> Execute(int ownerId, int tenantId);
}

public class RevokePermission : IRevokePermission
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RevokePermission(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<TransferPermissionResponseDto> Execute(int ownerId, int tenantId)
    {
        var owner = await _userRepository.GetByIdWithRole(ownerId);
        var tenant = await _userRepository.GetByIdWithRole(tenantId);

        if (owner == null || tenant == null)
        {
            return new TransferPermissionResponseDto
            {
                Success = false,
                Message = "Usuario no encontrado"
            };
        }

        if (owner.Role.Description != "Propietario" || tenant.Role.Description != "Inquilino")
        {
            return new TransferPermissionResponseDto
            {
                Success = false,
                Message = "Los roles no son válidos para esta operación"
            };
        }

        if (!tenant.HasPermission)
        {
            return new TransferPermissionResponseDto
            {
                Success = false,
                Message = "El inquilino no tiene permisos activos para revocar"
            };
        }

        owner.HasPermission = true;
        tenant.HasPermission = false;

        await _userRepository.Update(owner);
        await _userRepository.Update(tenant);

        await _refreshTokenRepository.RevokeAllByUserId(tenantId);

        return new TransferPermissionResponseDto
        {
            Success = true,
            Message = "Permisos revocados exitosamente. El inquilino debe iniciar sesión nuevamente.",
            OwnerId = ownerId,
            TenantId = tenantId,
            OwnerHasPermission = true,
            TenantHasPermission = false
        };
    }
}