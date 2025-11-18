using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public class GetActiveReserveCount
{
    private readonly IReserveRepository _repository;

    public GetActiveReserveCount(IReserveRepository repository)
    {
        _repository = repository;
    }


    public async Task<int> ExecuteAsync(DateTime? dateTime = null)
    {
        var now = dateTime ?? DateTime.Now;

        var activeReservations = await _repository.GetActiveReservationsAsync( now);
        return activeReservations.Count();
    }
}
