using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;

namespace Foraria.Application.UseCase
{
    public class CreateVote
    {

        private readonly IVoteRepository _voteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly ISignalRNotification _signalRNotification;


        public CreateVote(IVoteRepository voteRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, ISignalRNotification signalRNotification)
        {
            _voteRepository = voteRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _signalRNotification = signalRNotification;
        }

        public async Task ExecuteAsync(Vote vote)
        {
            var user = await _userRepository.GetById(vote.User_id);
            if (user == null)
            {
                throw new NotFoundException($"El usuario con ID {vote.User_id} no existe.");
            }

            var existingVote = await _voteRepository.GetByUserAndPollAsync(vote.User_id, vote.Poll_id);
            if (existingVote != null)
            {
                throw new InvalidOperationException("El usuario ya votó en esta encuesta.");
            }

            await _voteRepository.AddAsync(vote);
            await _unitOfWork.SaveChangesAsync();


            var updatedResults = await _voteRepository.GetPollResultsAsync(vote.Poll_id);


           await _signalRNotification.NotifyPollUpdatedAsync(vote.Poll_id, updatedResults);
        }
    }

}

