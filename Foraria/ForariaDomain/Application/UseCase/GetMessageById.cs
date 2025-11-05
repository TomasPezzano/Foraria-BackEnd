using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class GetMessageById
    {
        private readonly IMessageRepository _repository;

        public GetMessageById(IMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<Message?> Execute(int id)
        {
            return await _repository.GetById(id);
        }
    }
}