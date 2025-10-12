using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase
{
    public class GetAllPollsWithResults
    {
        private readonly IPollRepository _pollRepository;

        public GetAllPollsWithResults(IPollRepository pollRepository)
        {
            _pollRepository = pollRepository;
        }

        public async Task<List<Poll>> ExecuteAsync()
        {
            var polls = await _pollRepository.GetAllPollsWithResultsAsync();
            return polls;
        }

    }
}
