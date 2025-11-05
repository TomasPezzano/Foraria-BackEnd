using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace Foraria.Application.UseCase
{
    public class GetThreadById
    {
        private readonly IThreadRepository _repository;

        public GetThreadById(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<ForariaDomain.Thread?> Execute(int id)
        {
            var thread = await _repository.GetById(id);

            if (thread == null)
                throw new NotFoundException($"No se encontró el hilo con ID {id}.");

            return thread;

        }
    }
}
