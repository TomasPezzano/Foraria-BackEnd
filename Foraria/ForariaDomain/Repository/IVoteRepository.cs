using ForariaDomain;
using ForariaDomain.Models;

namespace Foraria.Domain.Repository
{
    public interface IVoteRepository
    {
        Task AddAsync(Vote vote);
        Task<Vote?> GetByUserAndPollAsync(int userId, int pollId);

        Task<IEnumerable<PollResult>> GetPollResultsAsync(int pollId);

        Task<IEnumerable<Vote>> GetVotesByPollIdAsync(int pollId);
    }
}
