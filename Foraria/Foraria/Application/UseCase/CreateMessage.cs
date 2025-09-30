using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using System.Threading.Tasks;

namespace Foraria.Application.UseCase
{
    public class CreateMessage
    {
        private readonly IMessageRepository _repository;

        public CreateMessage(IMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<MessageResponse> Execute(CreateMessageRequest request)
        {
            var message = new Message
            {
                Content = request.Content,
                Thread_id = request.Thread_id,
                User_id = request.User_id,
                optionalFile = request.optionalFile,
                CreatedAt = DateTime.UtcNow,
                State = "active"
            };

            var createdMessage = await _repository.Add(message);

            return new MessageResponse
            {
                Id = createdMessage.Id,
                Content = createdMessage.Content,
                CreatedAt = createdMessage.CreatedAt,
                State = createdMessage.State,
                Thread_id = createdMessage.Thread_id,
                User_id = createdMessage.User_id,
                optionalFile = createdMessage.optionalFile
            };
        }
    }
}