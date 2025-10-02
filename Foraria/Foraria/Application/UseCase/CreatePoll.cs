using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Exceptions;

namespace Foraria.Application.UseCase
{
    public class CreatePoll
    {

        private readonly IPollRepository _pollRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        public CreatePoll(IPollRepository pollRepository, IUnitOfWork unitOfWork, IUserRepository userRepository)
        {
            _pollRepository = pollRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<Poll> ExecuteAsync(PollDto request)
        {

            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
            {
                throw new NotFoundException($"El usuario con ID {request.UserId} no existe.");
            }

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
