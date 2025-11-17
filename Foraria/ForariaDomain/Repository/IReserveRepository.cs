using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IReserveRepository
    {
        Task<IEnumerable<Reserve>> GetUpcomingReservationsAsync(DateTime fromDate, int limit = 5);
        Task Add(Reserve reserve);
        Task<List<Reserve>> GetAllInConsortium();
        Task<List<Reserve>> GetAll();
        Task UpdateRange(List<Reserve> reserves);
        Task<IEnumerable<Reserve>> GetActiveReservationsAsync(DateTime now);

        Task<Reserve> getReserveByPlaceAndCreatedAt(DateTime createdAt, int place_id);

    }
}