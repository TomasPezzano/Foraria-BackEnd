using Azure.Core;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Exceptions;

namespace Foraria.Application.UseCase
{
    public class CreateVote
    {

        private readonly IVoteRepository _voteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        public CreateVote(IVoteRepository voteRepository, IUnitOfWork unitOfWork, IUserRepository userRepository)
        {
            _voteRepository = voteRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync(int userId, int pollId, int pollOptionId)
        {
            var user = await _userRepository.GetById(userId);

            if (user == null){
                throw new NotFoundException($"El usuario con ID {userId} no existe.");
            }

            var existingVote = await _voteRepository.GetByUserAndPollAsync(userId, pollId);
            if (existingVote != null)
            {
                throw new InvalidOperationException("El usuario ya votó en esta encuesta.");
            }

            var vote = new Vote
            {
                User_id = userId,
                Poll_id = pollId,
                PollOption_id = pollOptionId,
                VotedDate = DateTime.UtcNow
            };

            await _voteRepository.AddAsync(vote);
            await _unitOfWork.SaveChangesAsync();
        }
    }

}

