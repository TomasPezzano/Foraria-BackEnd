using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;


namespace ForariaDomain.Application.UseCase;

public class GetForumWithThreads
{
    private readonly IForumRepository _repository;

    public GetForumWithThreads(IForumRepository repository)
    {
        _repository = repository;
    }

    public async Task<Forum?> Execute(int id)
    {
        var forum = await _repository.GetByIdWithThreadsAsync(id)
            ?? throw new NotFoundException($"No se encontró el foro con ID {id}.");

        return forum;
    }
}
