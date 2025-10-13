using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using Foraria.Interface.DTOs.Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class GetMessagesByUser
    {
        private readonly IMessageRepository _repository;

        public GetMessagesByUser(IMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MessageDto>> ExecuteAsync(int userId)
        {
            var messages = await _repository.GetByUserIdAsync(userId);

            return messages.Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                State = m.State,
                OptionalFile = m.optionalFile,
                UserId = m.User_id
            });
        }
    }
}
