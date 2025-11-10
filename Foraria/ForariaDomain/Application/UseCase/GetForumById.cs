using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;
using ForariaDomain.Models;

namespace ForariaDomain.Application.UseCase;

public class GetForumById
{
    private readonly IForumRepository _repository;

    public GetForumById(IForumRepository repository)
    {
        _repository = repository;
    }

    public async Task<ForumWithStats?> Execute(int id)
    {
        var forum = await _repository.GetById(id)
            ?? throw new NotFoundException($"No se encontró el foro con ID {id}.");

        var countThreads = await _repository.TotalThreads(id);
        var countResponses = await _repository.TotalResponses(id);
        var countUserActives = await _repository.TotalUniqueParticipantsIncludingThreadCreators(id);

        return new ForumWithStats
        {
            Id = forum.Id,
            Category = forum.Category,
            CountThreads = countThreads,
            CountResponses = countResponses,
            CountUserActives = countUserActives
        };
    }
}
