using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Infrastructure.Persistence;

public class PlaceRepository : IPlaceRepository
{
    private readonly ForariaContext _context;
    public PlaceRepository(ForariaContext context)
    {
        _context = context;
    }
    public async Task<Place?> GetById(int id)
    {
        return await _context.Places
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
