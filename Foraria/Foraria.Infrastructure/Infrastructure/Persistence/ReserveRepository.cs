using Foraria.Domain.Repository;
using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Repository
{
    public class ReserveRepository : IReserveRepository
    {
        private readonly ForariaContext _context;

        public ReserveRepository(ForariaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reserve>> GetUpcomingReservationsAsync(
                    int consortiumId, DateTime fromDate, int limit = 5)
        {
            return await _context.Reserves
                .Include(r => r.Residence)
                    .ThenInclude(res => res.Consortium)
                .Include(r => r.User)
                .Include(r => r.Place)
                .Where(r =>
                    r.Residence.Consortium.Id == consortiumId &&
                    r.Date >= fromDate &&
                    r.State == "Confirmed") //open?
                .OrderBy(r => r.Date)
                .Take(limit)
                .ToListAsync();
        }
    }
}
