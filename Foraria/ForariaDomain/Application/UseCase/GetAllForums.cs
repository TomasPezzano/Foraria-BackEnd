using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaDomain.Application.UseCase;

public class GetAllForums
{
    private readonly IForumRepository _repository;

    public GetAllForums(IForumRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Forum>> Execute()
    {
        var forums = await _repository.GetAll();

        if (forums == null || !forums.Any())
            throw new NotFoundException("No se encontraron foros disponibles.");

        return forums.Where(f => f.IsActive);
    }
}
