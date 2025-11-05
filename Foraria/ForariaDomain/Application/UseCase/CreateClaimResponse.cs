using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Models;

namespace Foraria.Application.UseCase;

public interface ICreateClaimResponse
{
    Task<ClaimResponsResult> Execute(ClaimResponse claimResponse);
}

public class CreateClaimResponse : ICreateClaimResponse
{
    private readonly IClaimResponseRepository _claimResponseRepository;
    private readonly IClaimRepository _claimRepository;
    public CreateClaimResponse(
        IClaimResponseRepository claimResponseRepository,
        IClaimRepository claimRepository)
    {
        _claimResponseRepository = claimResponseRepository;
        _claimRepository = claimRepository;
    }

    public async Task<ClaimResponsResult> Execute(ClaimResponse claimResponse)
    {
        if (claimResponse.Claim == null || claimResponse.User == null)
            throw new ArgumentException("ClaimResponse incompleto");

        if (claimResponse.ResponsibleSector_id <= 0)
            throw new ArgumentException("Sector responsable inválido");

        await _claimResponseRepository.Add(claimResponse);

        claimResponse.Claim.State = "En Proceso";
        claimResponse.Claim.ClaimResponse = claimResponse;
        await _claimRepository.Update(claimResponse.Claim);

        var resultDto = new ClaimResponsResult
        {
            Id = claimResponse.Id,
            Description = claimResponse.Description,
            ResponseDate = claimResponse.ResponseDate,
            User_id = claimResponse.User.Id,
            Claim_id = claimResponse.Claim.Id,
            ResponsibleSector_id = claimResponse.ResponsibleSector_id
        };

        return resultDto;
    }
}
