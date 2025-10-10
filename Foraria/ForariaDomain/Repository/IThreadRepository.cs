namespace Foraria.Domain.Repository
{
    using ForariaDomain;
    using Microsoft.EntityFrameworkCore;

    public interface IThreadRepository
    {
        Task Add(Thread thread);
        Task<Thread?> GetById(int id);
    }
}
