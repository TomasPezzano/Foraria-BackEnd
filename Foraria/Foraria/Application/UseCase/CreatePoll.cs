using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class CreatePoll
    {

        private readonly IPollRepository _pollRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePoll(IPollRepository pollRepository, IUnitOfWork unitOfWork)
        {
            _pollRepository = pollRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Poll> ExecuteAsync(PollDto request)
        {
            var poll = new Poll
            {
                Title = request.Title,
                Description = request.Description,
                CategoryPoll_id = request.CategoryPollId,
                User_id = request.UserId,
                CreatedAt = DateTime.UtcNow,
                State = "Activa", 
                PollOptions = request.Options.Select(optionText => new PollOption
                {
                    Text = optionText
                }).ToList()
            };

            await _pollRepository.CreatePoll(poll);
            await _unitOfWork.SaveChangesAsync();
            return poll;
        }
    }
}
