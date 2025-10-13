using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using Foraria.Interface.DTOs.Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class GetThreadWithMessages
    {
        private readonly IThreadRepository _repository;

        public GetThreadWithMessages(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<ThreadWithMessagesDto?> ExecuteAsync(int id)
        {
            var thread = await _repository.GetByIdWithMessagesAsync(id);
            if (thread == null)
                return null;

            return new ThreadWithMessagesDto
            {
                Id = thread.Id,
                Theme = thread.Theme,
                Description = thread.Description,
                CreatedAt = thread.CreatedAt,
                State = thread.State,
                UserId = thread.User_id,
                ForumId = thread.Forum_id,
                Messages = thread.Messages.Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt,
                    State = m.State,
                    OptionalFile = m.optionalFile,
                    UserId = m.User_id
                }).ToList()
            };
        }
    }
}
