using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class CreateVote
    {

        private readonly IVoteRepository _voteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateVote(IVoteRepository voteRepository, IUnitOfWork unitOfWork)
        {
            _voteRepository = voteRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(int userId, int pollId, int pollOptionId)
        {
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

