using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaDomain.Application.UseCase;

public interface IRevokePermission
{
    Task Execute(int ownerId, int tenantId);
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

    public async Task Execute(int ownerId, int tenantId)
    {
        var owner = await _userRepository.GetByIdWithRole(ownerId);
        var tenant = await _userRepository.GetByIdWithRole(tenantId);

        if (owner == null)
        {
            throw new NotFoundException("Propietario no encontrado");
        }

        if (tenant == null)
        {
            throw new NotFoundException("Inquilino no encontrado");
        }

        if (owner.Role.Description != "Propietario")
        {
            throw new BusinessException("El usuario especificado no es un Propietario");
        }

        if (tenant.Role.Description != "Inquilino")
        {
            throw new BusinessException("El usuario especificado no es un Inquilino");
        }

        if (tenant.HasPermission == false)
        {
            throw new BusinessException("El inquilino no tiene permisos activos para revocar");
        }

        owner.HasPermission = true;
        tenant.HasPermission = false;

        await _userRepository.Update(owner);
        await _userRepository.Update(tenant);

        await _refreshTokenRepository.RevokeAllByUserId(tenantId);

    }
}