using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetPendingClaimsCount
    {
        private readonly IClaimRepository _repository;

        public GetPendingClaimsCount(IClaimRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> ExecuteAsync(int? consortiumId = null)
        {
            return await _repository.GetPendingCountAsync(consortiumId);
        }
    }
}
