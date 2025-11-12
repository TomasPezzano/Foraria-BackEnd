using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public class GetLatestPendingClaim
{
    private readonly IClaimRepository _repository;

    public GetLatestPendingClaim(IClaimRepository repository)
    {
        _repository = repository;
    }

    public async Task<object?> ExecuteAsync(int? consortiumId = null)
    {
        var claim = await _repository.GetLatestPendingAsync(consortiumId);

        if (claim == null)
            return null;

        return new
        {
            id = claim.Id,
            title = claim.Title,
            description = claim.Description,
            priority = claim.Priority,
            category = claim.Category,
            user = claim.User != null ? $"{claim.User.Name} {claim.User.LastName}" : null,
            residence = claim.Residence?.Number,
            consortium = claim.Residence?.Consortium?.Name,
            createdAt = claim.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }
}
