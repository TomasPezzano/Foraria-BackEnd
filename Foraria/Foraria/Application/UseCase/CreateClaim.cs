using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public class CreateClaim
{
    private readonly IClaimRepository _claimRepository;
    public CreateClaim(IClaimRepository claimRepository) {
        _claimRepository = claimRepository;
    }
    public void Execute(string Title, string Description, string Priority, string Category, string? Archive, int? User_id)
    {
        Claim claim = new Claim
        {
            Title = Title,
            Description = Description,
            State = "Nuevo",
            Priority = Priority,
            Category = Category,
            CreatedAt = DateTime.Now,
            Archive = Archive,
            User_id = User_id
        };

        _claimRepository.Add(claim);
    }

}
