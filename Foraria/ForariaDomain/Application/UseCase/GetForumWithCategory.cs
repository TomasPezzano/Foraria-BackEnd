using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaDomain.Application.UseCase;

public class GetForumWithCategory
{
    private readonly IForumRepository _repository;

    public GetForumWithCategory(IForumRepository repository)
    {
        _repository = repository;
    }

    public async Task<Forum?> Execute(int id)
    {
        var forum = await _repository.GetById(id)
            ?? throw new NotFoundException($"No se encontró el foro con ID {id}.");

        return forum;
    }
}
