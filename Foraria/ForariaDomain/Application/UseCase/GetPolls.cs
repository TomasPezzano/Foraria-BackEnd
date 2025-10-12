using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class GetPolls
    {
        private readonly IPollRepository _pollRepository;

        public GetPolls(IPollRepository pollRepository)
        {
            _pollRepository = pollRepository;
        }

        public async Task<List<Poll>> ExecuteAsync()
        {
            List<Poll> polls = await _pollRepository.GetAllPolls();

            return polls;

        }
    }
}
