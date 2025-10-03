using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IReactionRepository
    {
        Task<Reaction?> GetById(int id);
        Task Add(Reaction reaction);
        Task Remove(Reaction reaction);
        Task<int> CountByMessage(int messageId);
        Task<int> CountByThread(int threadId);
        Task<Reaction?> GetByUserAndTarget(int userId, int? messageId, int? threadId);
    }
}
