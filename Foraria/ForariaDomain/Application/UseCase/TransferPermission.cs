using Foraria.Contracts.DTOs;
using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public interface ITransferPermission
{
    Task<TransferPermissionResponseDto> Execute(int ownerId, int tenantId);
}

public class TransferPermission : ITransferPermission
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public TransferPermission(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository)
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

        if (owner.Role.Description != "Propietario")
        {
            return new TransferPermissionResponseDto
            {
                Success = false,
                Message = "Solo los propietarios pueden transferir permisos"
            };
        }

        if (tenant.Role.Description != "Inquilino")
        {
            return new TransferPermissionResponseDto
            {
                Success = false,
                Message = "Los permisos solo pueden transferirse a inquilinos"
            };
        }

        var ownerResidenceIds = owner.Residences?.Select(r => r.Id).ToList() ?? new List<int>();
        var tenantResidenceIds = tenant.Residences?.Select(r => r.Id).ToList() ?? new List<int>();

        var sharedResidences = ownerResidenceIds.Intersect(tenantResidenceIds).Any();

        if (!sharedResidences)
        {
            return new TransferPermissionResponseDto
            {
                Success = false,
                Message = "El propietario y el inquilino deben compartir al menos una residencia"
            };
        }

        if (!owner.HasPermission)
        {
            return new TransferPermissionResponseDto
            {
                Success = false,
                Message = "El propietario no tiene permisos de votación para transferir"
            };
        }

        owner.HasPermission = false;
        tenant.HasPermission = true;

        await _userRepository.Update(owner);
        await _userRepository.Update(tenant);

        await _refreshTokenRepository.RevokeAllByUserId(tenantId);

        return new TransferPermissionResponseDto
        {
            Success = true,
            Message = "Permisos transferidos exitosamente. El inquilino debe iniciar sesión nuevamente para obtener los nuevos permisos.",
            OwnerId = ownerId,
            TenantId = tenantId,
            OwnerHasPermission = false,
            TenantHasPermission = true
        };
    }
}