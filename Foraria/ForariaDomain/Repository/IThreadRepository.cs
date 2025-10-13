namespace Foraria.Domain.Repository
{
    using ForariaDomain;
    using Microsoft.EntityFrameworkCore;

    public interface IThreadRepository
    {
        Task Add(Thread thread);
        Task<Thread?> GetById(int id);
        Task<IEnumerable<Thread>> GetAllAsync();
        Task<IEnumerable<Thread>> GetByForumIdAsync(int forumId);
        Task UpdateAsync(Thread thread);
        Task DeleteAsync(int id);
        Task<Thread?> GetByIdWithMessagesAsync(int id);
    }
}
