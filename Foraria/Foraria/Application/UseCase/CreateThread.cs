using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using System.Threading.Tasks;
using Thread = ForariaDomain.Thread;

namespace Foraria.Application.UseCase
{
    public class CreateThread
    {
        private readonly IThreadRepository _repository;

        public CreateThread(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<ThreadResponse> Execute(CreateThreadRequest request)
        {
            var thread = new Thread
            {
                Theme = request.Theme,
                Description = request.Description,
                Forum_id = request.Forum_id,
                User_id = request.User_id,
                CreatedAt = DateTime.UtcNow,
                State = "Active"
            };

            await _repository.Add(thread);

            return new ThreadResponse
            {
                Id = thread.Id,
                Theme = thread.Theme,
                Description = thread.Description,
                CreatedAt = thread.CreatedAt,
                State = thread.State,
                Forum_id = thread.Forum_id,
                User_id = thread.User_id
            };
        }
    }
}