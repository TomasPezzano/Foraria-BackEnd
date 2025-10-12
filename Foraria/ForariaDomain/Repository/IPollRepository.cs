using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IPollRepository
    {

        Task CreatePoll(Poll poll); 

        Task<List<Poll>> GetAllPolls();
        Task<Poll?> GetById(int id);
        Task<IEnumerable<Poll>> GetActivePolls(int consortiumId, DateTime now);


    }
}
