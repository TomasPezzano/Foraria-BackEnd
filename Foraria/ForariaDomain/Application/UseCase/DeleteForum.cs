using ForariaDomain.Exceptions;
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
                throw new NotFoundException($"No se encontró el foro con ID {id}.");

            if (!forum.IsActive)
                throw new BusinessException("El foro ya está deshabilitado.");

            if (forum.Threads.Any(t => t.State == "Active"))
                throw new BusinessException("No se puede deshabilitar el foro porque contiene threads activos.");

            forum.IsActive = false;
            await _repository.UpdateAsync(forum);
        }
    }
}
