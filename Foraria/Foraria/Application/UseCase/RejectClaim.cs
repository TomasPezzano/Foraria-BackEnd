using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase;

public class RejectClaim
{

    private readonly IClaimRepository _claimRepository;
    public RejectClaim(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }
    public void Execute(int claimId)
    {
        var claim = _claimRepository.GetById(claimId);
        if (claim == null)
        {
            throw new ArgumentException($"No se encontró un reclamo con ID {claimId}");
        }
        if (claim.State == "Rechazado")
        {
            throw new InvalidOperationException($"El reclamo con ID {claimId} ya está rechazado");
        }
        claim.State = "Rechazado";
        _claimRepository.Update(claim);
    }
}
