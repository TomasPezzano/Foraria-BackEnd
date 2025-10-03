namespace Foraria.Domain.Repository
{
    using ForariaDomain;
    using Microsoft.EntityFrameworkCore;

    using global::Foraria.Infrastructure.Persistence;

    public interface IThreadRepository
    {
        Task Add(Thread thread);
        Task<Thread?> GetById(int id);
    }
}
