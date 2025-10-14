using Foraria.Domain.Repository;
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

        public async Task<Poll> ExecuteAsync(Poll poll)
        {
            var user = await _userRepository.GetById(poll.User_id);
            if (user == null)
            {
                throw new NotFoundException($"El usuario con ID {poll.User_id} no existe.");
            }

            await _pollRepository.CreatePoll(poll);

            await _unitOfWork.SaveChangesAsync();

            return poll;
        }
    }
}
