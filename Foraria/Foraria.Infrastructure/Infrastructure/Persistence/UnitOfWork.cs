using Foraria.Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ForariaContext _context;

        public UnitOfWork(ForariaContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }

}
