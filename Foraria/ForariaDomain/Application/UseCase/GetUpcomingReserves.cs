using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetUpcomingReserves
    {
        private readonly IReserveRepository _repository;

        public GetUpcomingReserves(IReserveRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> ExecuteAsync(int consortiumId, int limit = 5)
        {
            var now = DateTime.UtcNow;

            var reservations = await _repository.GetUpcomingReservationsAsync(consortiumId, now, limit);

            var list = reservations.Select(r => new
            {
                id = r.Id,
                place = r.Place?.Name,
                user = r.User != null ? $"{r.User.Name} {r.User.LastName}" : null,
                date = r.Date.ToString("yyyy-MM-dd"),
                time = r.Date.ToString("HH:mm")
            });

            return new
            {
                consortiumId,
                generatedAt = now.ToString("yyyy-MM-dd HH:mm:ss"),
                upcomingReservations = list
            };
        }
    }
}
