using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaDomain.Application.UseCase;

public class UpdatePoll
{
    private readonly IPollRepository _pollRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePoll(IPollRepository pollRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _pollRepository = pollRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Poll> ExecuteAsync(int pollId, int userId, Poll pollData)
    {
        var poll = await _pollRepository.GetById(pollId)
            ?? throw new NotFoundException($"La votación con ID {pollId} no existe.");

        var user = await _userRepository.GetById(userId)
            ?? throw new NotFoundException($"El usuario con ID {userId} no existe.");

        bool isOwner = poll.User_id == userId;
        bool isConsortium = user.Role.Description == "Consorcio";

        if (!isOwner && !isConsortium)
            throw new UnauthorizedAccessException("No tiene permisos para modificar esta votación.");

        if (poll.State == "Active" || poll.State == "Closed" || poll.State == "Rejected")
            throw new InvalidOperationException("No se pueden modificar votaciones activas, cerradas o rechazadas.");

        if (!string.IsNullOrWhiteSpace(pollData.Title))
            poll.Title = pollData.Title;

        if (!string.IsNullOrWhiteSpace(pollData.Description))
            poll.Description = pollData.Description;

        if (pollData.StartDate != default)
            poll.StartDate = pollData.StartDate;

        if (pollData.EndDate != default)
            poll.EndDate = pollData.EndDate;

        if (!string.IsNullOrWhiteSpace(pollData.State))
            poll.State = pollData.State;

        await _pollRepository.UpdatePoll(poll);
        await _unitOfWork.SaveChangesAsync();

        return poll;
    }
}
