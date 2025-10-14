using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class CloseThread
    {
        private readonly IThreadRepository _repository;

        public CloseThread(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<ThreadDto> ExecuteAsync(int id)
        {
            var thread = await _repository.GetById(id);
            if (thread == null)
                throw new InvalidOperationException($"No se encontró el thread con ID {id}");

            if (thread.State == "Closed")
                throw new InvalidOperationException("El thread ya se encuentra cerrado.");

            thread.State = "Closed";
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
