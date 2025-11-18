using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;
public interface ICreateReserve
{
    Task<Reserve> Execute(Reserve reserve);
}
public class CreateReserve : ICreateReserve
{
    private readonly IReserveRepository _reserveRepository;

    public CreateReserve(IReserveRepository reserveRepository)
    {
        _reserveRepository = reserveRepository;
    }

    public async Task<Reserve> Execute(Reserve reserve)
    {

        var obtainedReserve = await _reserveRepository.getReserveByPlaceAndCreatedAt(reserve.CreatedAt, reserve.Place_id);
        if(obtainedReserve != null) {
            if(reserve.CreatedAt == obtainedReserve.CreatedAt && reserve.Place_id == obtainedReserve.Place_id)
            {
                return null;
            }
        }

        reserve.Date = DateTime.Now;
        reserve.DeletedAt = reserve.CreatedAt.AddHours(1);
        await _reserveRepository.Add(reserve);
        return reserve;
    }
}
