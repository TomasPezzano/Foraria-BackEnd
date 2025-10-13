using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class HideMessage
    {
        private readonly IMessageRepository _repository;

        public HideMessage(IMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> ExecuteAsync(int id)
        {
            var message = await _repository.GetById(id);
            if (message == null)
                throw new InvalidOperationException($"No se encontró el mensaje con ID {id}");

            if (message.State == "Hidden")
                throw new InvalidOperationException("El mensaje ya se encuentra oculto.");

            message.State = "Hidden";
            await _repository.Update(message);
            return true;
        }
    }
}
