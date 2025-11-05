using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase;
public interface ICreateClaim
{
    Task<Claim> Execute(Claim claim);
}
public class CreateClaim : ICreateClaim
{
    private readonly IClaimRepository _claimRepository;
    
    public CreateClaim(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }
    public async Task<Claim> Execute(Claim claim)
    {
        if (string.IsNullOrWhiteSpace(claim.Title))
            throw new ArgumentException("El título del reclamo es obligatorio");
        if (string.IsNullOrWhiteSpace(claim.Description))
            throw new ArgumentException("La descripción del reclamo es obligatoria");
        if (string.IsNullOrWhiteSpace(claim.Priority))
            throw new ArgumentException("La prioridad es obligatoria");
        if (string.IsNullOrWhiteSpace(claim.Category))
            throw new ArgumentException("La categoría es obligatoria");
        if (claim.User_id == null)
            throw new ArgumentException("Debe asociarse un usuario al reclamo");

        await _claimRepository.Add(claim);
        return claim;
    }

}
