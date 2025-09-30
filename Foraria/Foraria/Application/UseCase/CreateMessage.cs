using ForariaDomain;
using Foraria.Domain.Repository.Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    namespace Foraria.Application.UseCase
    {
        public class CreateMessage
        {
            private readonly IMessageRepository _repository;

            public CreateMessage(IMessageRepository repository)
            {
                _repository = repository;
            }

            public async Task<Message> Execute(Message message)
            {
                message.CreatedAt = DateTime.UtcNow;
                message.State = "active"; // estado por defecto
                return await _repository.Add(message);
            }
        }
    }
}
