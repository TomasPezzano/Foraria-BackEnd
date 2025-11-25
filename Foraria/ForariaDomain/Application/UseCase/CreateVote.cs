using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaDomain.Application.UseCase;

public class CreateVote
{

    private readonly IVoteRepository _voteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ISignalRNotification _signalRNotification;
    private readonly IGetPollById _getPollById;


    public CreateVote(IVoteRepository voteRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, ISignalRNotification signalRNotification, IGetPollById getPollById)
    {
        _voteRepository = voteRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _signalRNotification = signalRNotification;
        _getPollById = getPollById;
    }

    public async Task ExecuteAsync(Vote vote)
    {
        var user = await _userRepository.GetById(vote.User_id);
        if (user == null)
        {
            throw new NotFoundException($"El usuario con ID {vote.User_id} no existe.");
        }

        var poll = await _getPollById.ExecuteAsync(vote.Poll_id);
        if (poll == null)
        {
            throw new NotFoundException($"La votacion con ID {vote.Poll_id} no existe.");
        }


        var existingVote = await _voteRepository.GetByUserAndPollAsync(vote.User_id, vote.Poll_id);
        if (existingVote != null)
        {
            throw new InvalidOperationException("El usuario ya votó en esta encuesta.");
        }

        if (poll.State.Equals("Finalizada") || poll.State.Equals("Pendiente"))
        {
            throw new InvalidOperationException("No se puede votar en una votacion en estado pendiente o finalizada");
        }

        if (user.Role.Description == "Inquilino")
        {
            if (user.HasPermission)
            {
                await _voteRepository.AddAsync(vote);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("El inquilino no tiene permiso para votar.");
            }
        }
        else
        {
            await _voteRepository.AddAsync(vote);
            await _unitOfWork.SaveChangesAsync();
        }


        var updatedResults = await _voteRepository.GetPollResultsAsync(vote.Poll_id);


        await _signalRNotification.NotifyPollUpdatedAsync(vote.Poll_id, updatedResults);
    }
}

