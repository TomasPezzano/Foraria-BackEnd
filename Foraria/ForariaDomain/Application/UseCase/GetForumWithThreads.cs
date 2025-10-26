using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class GetForumWithThreads
    {
        private readonly IForumRepository _repository;

        public GetForumWithThreads(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task<ForumDto?> Execute(int id)
        {
            var forum = await _repository.GetByIdWithThreadsAsync(id);
            if (forum == null) return null;

            return new ForumDto
            {
                Id = forum.Id,
                Category = forum.Category,
                Threads = forum.Threads.Select(t => new ThreadDto
                {
                    Id = t.Id,
                    Theme = t.Theme,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    State = t.State,
                    UserId = t.UserId
                }).ToList()
            };
        }
    }
}
