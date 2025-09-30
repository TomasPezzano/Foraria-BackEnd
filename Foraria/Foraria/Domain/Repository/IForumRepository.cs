namespace Foraria.Domain.Repository
{
    using ForariaDomain;
    using System.Threading.Tasks;

    public interface IForumRepository
    {
        Task<Forum> Add(Forum forum);
    }
}
