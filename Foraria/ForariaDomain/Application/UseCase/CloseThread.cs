using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace Foraria.Application.UseCase
{
    public class CloseThread
    {
        private readonly IThreadRepository _repository;

        public CloseThread(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<ForariaDomain.Thread> ExecuteAsync(int id)
        {
            var thread = await _repository.GetById(id)
                       ?? throw new NotFoundException($"No se encontró el hilo con ID {id}");

            if (thread.State == "Closed")
                throw new ThreadLockedException("El hilo ya se encuentra cerrado.");

            thread.State = "Closed";
            thread.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(thread);

            return thread;
        }
    }
}
