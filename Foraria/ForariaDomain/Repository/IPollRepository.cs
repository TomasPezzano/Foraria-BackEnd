using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IPollRepository
    {

        Task CreatePoll(Poll poll);

        Task<List<Poll>> GetAllPolls();
        Task<Poll?> GetById(int id);

        Task<Poll?> GetPollWithResultsAsync(int pollId);

        Task<List<Poll>> GetAllPollsWithResultsAsync();
        Task UpdatePoll(Poll poll);

        Task<IEnumerable<Poll>> GetActivePolls(int consortiumId, DateTime now);

        Task<Poll> AddAsync(Poll poll);
        Task<Poll?> GetByIdAsync(int id);
        Task<IEnumerable<Poll>> GetByConsortiumIdAsync();
        Task<IEnumerable<Poll>> GetActiveByConsortiumIdAsync();
        Task<IEnumerable<Poll>> GetClosingSoonAsync(DateTime startDate, DateTime endDate);
        Task UpdateAsync(Poll poll);
        Task DeleteAsync(int id);
        Task<bool> HasUserVotedAsync(int pollId, int userId);
        Task<int> GetTotalVotesAsync(int pollId);
    }
}
