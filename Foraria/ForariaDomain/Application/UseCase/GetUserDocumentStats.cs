using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class GetUserDocumentStats
    {
        private readonly IUserDocumentRepository _repository;

        public GetUserDocumentStats(IUserDocumentRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserDocumentStatsDto> ExecuteAsync(int? userId = null)
        {
            return await _repository.GetStatsAsync(userId);
        }
    }
}
