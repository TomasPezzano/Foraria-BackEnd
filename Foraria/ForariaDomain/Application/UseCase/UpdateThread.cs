using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class UpdateThread
    {
        private readonly IThreadRepository _repository;

        public UpdateThread(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<ThreadDto> ExecuteAsync(int id, UpdateThreadRequest request)
        {
            var thread = await _repository.GetById(id);
            if (thread == null)
                throw new InvalidOperationException($"No se encontró el thread con ID {id}");

            if (!string.IsNullOrWhiteSpace(request.Theme))
                thread.Theme = request.Theme;

            if (!string.IsNullOrWhiteSpace(request.Description))
                thread.Description = request.Description;

            if (!string.IsNullOrWhiteSpace(request.State))
                thread.State = request.State;

            await _repository.UpdateAsync(thread);

            return new ThreadDto
            {
                Id = thread.Id,
                Theme = thread.Theme,
                Description = thread.Description,
                CreatedAt = thread.CreatedAt,
                State = thread.State,
                UserId = thread.User_id,
                ForumId = thread.Forum_id
            };
        }
    }
}
