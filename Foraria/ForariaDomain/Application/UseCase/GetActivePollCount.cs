using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetActivePollCount
    {
        private readonly IPollRepository _repository;

        public GetActivePollCount(IPollRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> ExecuteAsync(int consortiumId, DateTime? dateTime = null)
        {
            var now = dateTime ?? DateTime.UtcNow;
            var activePolls = await _repository.GetActivePolls(consortiumId, now);
            return activePolls.Count();
        }
    }
}