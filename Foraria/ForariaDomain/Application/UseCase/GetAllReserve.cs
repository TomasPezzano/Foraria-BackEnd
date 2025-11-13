using Foraria.Domain.Repository;


namespace ForariaDomain.Application.UseCase;

public interface IGetAllReserve
{
    Task<List<Reserve>> Execute(int idConsortium);
}
public class GetAllReserve : IGetAllReserve
{
    private readonly IReserveRepository _reserveRepository;
    public GetAllReserve(IReserveRepository reserveRepository)
    {
        _reserveRepository = reserveRepository;
    }
    public async Task<List<Reserve>> Execute(int idConsortium)
    {
       return await _reserveRepository.GetAllInConsortium(idConsortium);
    }
}

