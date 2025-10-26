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

        public async Task Add(Reserve reserve)
        {
            _context.Reserves.Add(reserve);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Reserve>> GetAll()
        {
            return await _context.Reserves.ToListAsync();
        }

        public async Task UpdateRange(List<Reserve> reserves)
        {
            _context.Reserves.UpdateRange(reserves);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Reserve>> GetActiveReservationsAsync(int consortiumId, DateTime now)
        {
            return await _context.Reserves
                .Include(r => r.Place)
                .Include(r => r.User)
                .Include(r => r.Residence)
                .Where(r =>
                    r.Residence.ConsortiumId == consortiumId &&
                    r.State == "active" &&             // activa/Active(?)
                    r.DeletedAt == null &&
                    r.Date >= now)
                .ToListAsync();
        }
    }
}
