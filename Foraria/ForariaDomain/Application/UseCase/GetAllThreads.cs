using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class GetAllThreads
    {
        private readonly IThreadRepository _repository;

        public GetAllThreads(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ThreadDto>> ExecuteAsync(int? forumId = null)
        {
            var threads = forumId.HasValue
                ? await _repository.GetByForumIdAsync(forumId.Value)
                : await _repository.GetAllAsync();

            return threads.Select(t => new ThreadDto
            {
                Id = t.Id,
                Theme = t.Theme,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                State = t.State,
                UserId = t.User_id,
                ForumId = t.Forum_id
            }).ToList();
        }
    }
}
