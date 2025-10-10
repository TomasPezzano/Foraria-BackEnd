using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class DeleteMessage
    {
        private readonly IMessageRepository _repository;

        public DeleteMessage(IMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Execute(int id)
        {
            var message = await _repository.GetById(id);
            if (message == null)
                return false;

            await _repository.Delete(message);
            return true;
        }
    }
}