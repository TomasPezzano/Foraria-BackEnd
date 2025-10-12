using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Repository;

namespace Foraria.Infrastructure.Infrastructure.Persistence;

public class ReserveImplementation : IReserveRepository
{
    public readonly ForariaContext _context;

    public ReserveImplementation(ForariaContext context)
    {
        _context = context;
    }

    public async Task Add(Reserve reserve)
    {
        _context.Reserves.Add(reserve);
        await _context.SaveChangesAsync();
    }
}
