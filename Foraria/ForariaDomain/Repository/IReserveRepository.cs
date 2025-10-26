using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IReserveRepository
    {
        Task<IEnumerable<Reserve>> GetUpcomingReservationsAsync(int consortiumId, DateTime fromDate, int limit = 5);
        Task Add(Reserve reserve);
        Task<List<Reserve>> GetAll();
        Task UpdateRange(List<Reserve> reserves);
        Task<IEnumerable<Reserve>> GetActiveReservationsAsync(int consortiumId, DateTime now);


    }
}