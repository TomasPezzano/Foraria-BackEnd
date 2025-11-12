using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetConsortiumById {
    Task<Consortium> Execute(int id);
}
public class GetConsortiumById :  IGetConsortiumById 
{
    private readonly IConsortiumRepository _consortiumRepository;

    public GetConsortiumById(IConsortiumRepository consortiumRepository) {
    _consortiumRepository = consortiumRepository;
    }

    public async Task<Consortium> Execute(int id)
    {
        return await _consortiumRepository.FindById(id);
    }
}
