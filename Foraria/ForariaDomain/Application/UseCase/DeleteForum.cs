using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class DeleteForum
    {
        private readonly IForumRepository _repository;

        public DeleteForum(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(int id)
        {
            var forum = await _repository.GetByIdWithThreadsAsync(id);
            if (forum == null)
                throw new InvalidOperationException($"No se encontró el foro con ID {id}");

            if (forum.Threads.Any())
                throw new InvalidOperationException("No se puede eliminar el foro porque contiene threads activos.");

            await _repository.Delete(id);
        }
    }
}
