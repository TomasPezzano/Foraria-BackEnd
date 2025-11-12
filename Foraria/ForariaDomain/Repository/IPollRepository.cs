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


    }
}
