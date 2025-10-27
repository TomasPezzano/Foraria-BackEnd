using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Infrastructure.Persistence
{
    public class ConsortiumRepository : IConsortiumRepository
    {
        private readonly ForariaContext _context;
        public ConsortiumRepository(ForariaContext context)
        {
            _context = context;
        }
        public async Task<Consortium> FindById(int id)
        {
            return await _context.Consortium.FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
