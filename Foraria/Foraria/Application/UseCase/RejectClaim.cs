using System.Runtime.CompilerServices;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public interface IRejectClaim
{
    Task Execute(int claimId);
}
public class RejectClaim : IRejectClaim
{

    private readonly IClaimRepository _claimRepository;
    public RejectClaim(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }
    public async Task  Execute(int claimId)
    {
        var claim = await _claimRepository.GetById(claimId);
        if (claim == null)
        {
            throw new ArgumentException($"No se encontró un reclamo con ID {claimId}");
        }
        if (claim.State == "Rechazado")
        {
            throw new InvalidOperationException($"El reclamo con ID {claimId} ya está rechazado");
        }
        claim.State = "Rechazado";
        await _claimRepository.Update(claim);
    }
}
