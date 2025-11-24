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

        public async Task<IEnumerable<Reserve>> GetUpcomingReservationsAsync(DateTime fromDate, int limit = 5)
        {
            return await _context.Reserves
                .Include(r => r.Consortium)
                .Include(r => r.User)
                .Include(r => r.Place)
                .Where(r =>
                    r.Date >= fromDate &&
                    r.State == "Confirmed") 
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
            return await _context.Reserves
                .ToListAsync();
        }

        public async Task<List<Reserve>> GetAllInConsortium()
        {
            return await _context.Reserves
                .Include(r => r.Place)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task UpdateRange(List<Reserve> reserves)
        {
            _context.Reserves.UpdateRange(reserves);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Reserve>> GetActiveReservationsAsync(DateTime now)
        {
            return await _context.Reserves
                .Include(r => r.Place)
                .Include(r => r.User)
                .Where(r =>
                    r.State == "Nuevo" &&
                    r.CreatedAt >= now &&  
                    r.DeletedAt > now)             
                .ToListAsync();
        }

        public async Task<Reserve> getReserveByPlaceAndCreatedAt(DateTime createdAt, int place_id)
        {
            return await _context.Reserves.Where(r=> r.CreatedAt == createdAt && r.Place_id == place_id ).FirstOrDefaultAsync();
        }
    }
}
