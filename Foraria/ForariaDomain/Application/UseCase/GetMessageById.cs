using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class GetMessageById
    {
        private readonly IMessageRepository _repository;

        public GetMessageById(IMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<MessageResponse?> Execute(int id)
        {
            var message = await _repository.GetById(id);
            if (message == null) return null;

            return new MessageResponse
            {
                Id = message.Id,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                State = message.State,
                Thread_id = message.Thread_id,
                User_id = message.User_id,
                optionalFile = message.optionalFile
            };
        }
    }
}