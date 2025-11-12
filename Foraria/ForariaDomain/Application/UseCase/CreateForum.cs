using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaDomain.Application.UseCase;

public class CreateForum
{
    private readonly IForumRepository _repository;

    public CreateForum(IForumRepository repository)
    {
        _repository = repository;
    }

    public async Task<Forum> Execute(Forum forum)
    {
        var existingForum = await _repository.GetByCategory(forum.Category);
        if (existingForum != null)
            throw new BusinessException($"Ya existe un foro para la categoría '{forum.Category}'.");

        var createdForum = await _repository.Add(forum);
        return createdForum;
    }
}