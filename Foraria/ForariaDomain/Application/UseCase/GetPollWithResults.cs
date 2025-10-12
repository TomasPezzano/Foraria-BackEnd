
using Foraria.Contracts.DTOs;
using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase
{
    public class GetPollWithResults
    {
        private readonly IPollRepository _pollRepository;

        public GetPollWithResults(IPollRepository pollRepository)
        {
            _pollRepository = pollRepository;
        }

        public async Task<Poll?> ExecuteAsync(int pollId)
        {
            var poll = await _pollRepository.GetPollWithResultsAsync(pollId);
            return poll;
        }
    }
}
