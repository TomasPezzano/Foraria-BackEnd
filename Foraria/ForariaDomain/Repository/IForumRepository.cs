namespace Foraria.Domain.Repository
{
    using ForariaDomain;
    using global::Foraria.Domain.Model;
    using System.Threading.Tasks;

    public interface IForumRepository
    {
        Task<Forum> Add(Forum forum);
        Task<Forum?> GetById(int id);
        Task<IEnumerable<Forum>> GetAll();
        Task<Forum?> GetByCategory(ForumCategory category);
        Task<Forum?> GetByIdWithThreadsAsync(int id);
        Task Delete(int id);


    }
}
