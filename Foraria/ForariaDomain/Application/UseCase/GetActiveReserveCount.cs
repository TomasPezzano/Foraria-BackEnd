using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetActiveReserveCount
    {
        private readonly IReserveRepository _repository;

        public GetActiveReserveCount(IReserveRepository repository)
        {
            _repository = repository;
        }


        public async Task<int> ExecuteAsync(int consortiumId, DateTime? dateTime = null)
        {
            var now = dateTime ?? DateTime.UtcNow;

            var activeReservations = await _repository.GetActiveReservationsAsync(consortiumId, now);
            return activeReservations.Count();
        }
    }
}
