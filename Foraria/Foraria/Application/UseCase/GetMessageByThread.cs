using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class GetMessagesByThread
    {
        private readonly IMessageRepository _repository;

        public GetMessagesByThread(IMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MessageResponse>> Execute(int threadId)
        {
            var messages = await _repository.GetByThread(threadId);

            return messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                State = m.State,
                Thread_id = m.Thread_id,
                User_id = m.User_id,
                optionalFile = m.optionalFile
            });
        }
    }
}
