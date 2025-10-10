using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class GetThreadById
    {
        private readonly IThreadRepository _repository;

        public GetThreadById(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<ThreadResponse?> Execute(int id)
        {
            var thread = await _repository.GetById(id);
            if (thread == null) return null;

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
