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

        reserve.DeletedAt = reserve.CreatedAt.AddHours(1);
        reserve.Date = DateTime.UtcNow;
        await _reserveRepository.Add(reserve);
        return reserve;
    }
}
