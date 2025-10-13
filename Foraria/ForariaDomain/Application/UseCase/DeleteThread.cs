using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class DeleteThread
    {
        private readonly IThreadRepository _repository;

        public DeleteThread(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(int id)
        {
            var thread = await _repository.GetByIdWithMessagesAsync(id);
            if (thread == null)
                throw new InvalidOperationException($"No se encontró el thread con ID {id}");

            if (thread.Messages != null && thread.Messages.Any())
                throw new InvalidOperationException("No se puede eliminar un thread que contiene mensajes.");

            await _repository.DeleteAsync(id);
        }
    }
}
