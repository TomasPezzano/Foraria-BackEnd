using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IReserveRepository
    {
        Task<IEnumerable<Reserve>> GetUpcomingReservationsAsync(int consortiumId, DateTime fromDate, int limit = 5);
    }
}