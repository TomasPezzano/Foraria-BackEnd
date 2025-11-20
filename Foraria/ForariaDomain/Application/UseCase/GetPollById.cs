using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetPollById
{
    Task<Poll?> ExecuteAsync(int id);
}

public class GetPollById : IGetPollById
{
    private readonly IPollRepository _repository;

    public GetPollById(IPollRepository repository)
    {
        _repository = repository;
    }

    public async Task<Poll?> ExecuteAsync(int id)
    {
        return await _repository.GetById(id);
    }
}
