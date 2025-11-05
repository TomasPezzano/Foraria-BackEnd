using Foraria.Domain.Repository;
using ForariaDomain;


namespace Foraria.Application.UseCase
{
    public class GetUserDocumentStats
    {
        private readonly IUserDocumentRepository _repository;

        public GetUserDocumentStats(IUserDocumentRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserDocument> ExecuteAsync(int? userId = null)
        {
            return await _repository.GetStatsAsync(userId);
        }
    }
}
