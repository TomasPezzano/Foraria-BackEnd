namespace Foraria.Domain.Repository
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
