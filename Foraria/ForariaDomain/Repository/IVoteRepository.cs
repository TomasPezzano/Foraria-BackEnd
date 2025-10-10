using Foraria.Application.UseCase;
using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IVoteRepository
    {
        Task AddAsync(Vote vote);
        Task<Vote?> GetByUserAndPollAsync(int userId, int pollId);
    }
}
