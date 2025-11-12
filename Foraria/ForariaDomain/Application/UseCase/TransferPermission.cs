using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;


namespace ForariaDomain.Application.UseCase;

public interface ITransferPermission
{
    Task Execute(int ownerId, int tenantId);
}

public class TransferPermission : ITransferPermission
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IResidenceRepository _residenceRepository;

    public TransferPermission(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IResidenceRepository residenceRepository)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _residenceRepository = residenceRepository;
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
            throw new BusinessException("Solo los propietarios pueden transferir permisos");
        }

        if (tenant.Role.Description != "Inquilino")
        {
            throw new BusinessException("Los permisos solo pueden transferirse a inquilinos");
        }

        var ownerResidenceIds = owner.Residences?.Select(r => r.Id).ToList() ?? new List<int>();
        var tenantResidenceIds = tenant.Residences?.Select(r => r.Id).ToList() ?? new List<int>();

        var residenceOwnerId = await _residenceRepository.GetResidenceByUserId(ownerId);
        var residenceTenantId = await _residenceRepository.GetResidenceByUserId(tenantId);

        bool sharedResidences = false;
        foreach (var residenceId in residenceOwnerId)
        {
            foreach (var residenceId2 in residenceTenantId)
            {
                if (residenceId == residenceId2) 
                    sharedResidences = true;
                break;
            }
        }

        if (!sharedResidences)
        {
            throw new BusinessException("El propietario y el inquilino deben compartir al menos una residencia");
        }

        if (owner.HasPermission != null && owner.HasPermission == true)
        {
            throw new BusinessException("El propietario no tiene permisos para transferir");
        }

        owner.HasPermission = false;
        tenant.HasPermission = true;

        await _userRepository.Update(owner);
        await _userRepository.Update(tenant);

        await _refreshTokenRepository.RevokeAllByUserId(tenantId);
    }
}