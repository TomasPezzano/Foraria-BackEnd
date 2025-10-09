using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class GetPollById
    {
        private readonly IPollRepository _repository;

        public GetPollById(IPollRepository repository)
        {
            _repository = repository;
        }

        public async Task<Poll?> ExecuteAsync(int id)
        {
            return await _repository.GetById(id);
        }
    }
}
