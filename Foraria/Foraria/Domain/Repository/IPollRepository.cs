using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IPollRepository
    {

        Task CreatePoll(Poll poll); 

        Task<List<Poll>> GetAllPolls();

    }
}
