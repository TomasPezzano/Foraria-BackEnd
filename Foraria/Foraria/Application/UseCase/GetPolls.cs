using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class GetPolls
    {
        private readonly IPollRepository _pollRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetPolls(IPollRepository pollRepository, IUnitOfWork unitOfWork)
        {
            _pollRepository = pollRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<PollDto>> ExecuteAsync()
        {
            List<Poll> polls = await _pollRepository.GetAllPolls();

            List<PollDto> pollsDto = polls.Select(p => new PollDto
            {
                Title = p.Title,
                Description = p.Description,
                CategoryPollId = p.CategoryPoll_id,
                UserId = p.User_id,
                Options = p.PollOptions != null
                            ? p.PollOptions.Select(o => o.Text).ToList()
                            : new List<string>()
            }).ToList();

            return pollsDto;

        }
    }
}
