using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public class CreateClaim
{
    private readonly IClaimRepository _claimRepository;
    public CreateClaim(IClaimRepository claimRepository) {
        _claimRepository = claimRepository;
    }
    public void Execute(ClaimDto claimDto)
    {
        Claim claim = new Claim
        {
            Title = claimDto.Title,
            Description = claimDto.Description,
            State = "Nuevo",
            Priority = claimDto.Priority,
            Category = claimDto.Category,
            CreatedAt = DateTime.Now,
            Archive = claimDto.Archive,
            User_id = claimDto.User_id
        };

        _claimRepository.Add(claim);
    }

}
