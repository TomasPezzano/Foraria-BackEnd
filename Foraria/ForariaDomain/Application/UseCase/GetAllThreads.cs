using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace Foraria.Application.UseCase
{
    public class GetAllThreads
    {
        private readonly IThreadRepository _repository;

        public GetAllThreads(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ForariaDomain.Thread>> ExecuteAsync(int? forumId = null)
        {
            var threads = forumId.HasValue
                ? await _repository.GetByForumIdAsync(forumId.Value)
                : await _repository.GetAllAsync();

            if (threads == null || !threads.Any())
                throw new NotFoundException("No se encontraron hilos.");

            return threads;
        }
    }
}
