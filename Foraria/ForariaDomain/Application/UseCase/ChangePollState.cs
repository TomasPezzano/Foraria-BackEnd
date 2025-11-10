using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;

namespace ForariaDomain.Application.UseCase;

public class ChangePollState
{
    private readonly IPollRepository _pollRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePollState(IPollRepository pollRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _pollRepository = pollRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Poll> ExecuteAsync(int pollId, int userId, string newState)
    {
        var poll = await _pollRepository.GetById(pollId)
            ?? throw new NotFoundException($"La votación con ID {pollId} no existe.");

        var user = await _userRepository.GetById(userId)
            ?? throw new NotFoundException($"El usuario con ID {userId} no existe.");

        if (user.Role.Description != "Consorcio")
            throw new UnauthorizedAccessException("Solo los usuarios con rol Consorcio pueden cambiar el estado de votaciones.");

        if (poll.State != "Pendiente")
            throw new InvalidOperationException("Solo pueden modificarse votaciones en estado Pendiente.");

        if (newState != "Activa" && newState != "Rechazada")
            throw new ArgumentException("El nuevo estado debe ser 'Activa' o 'Rechazada'.");

        poll.State = newState;
        poll.ApprovedByUserId = user.Id;
        poll.ApprovedAt = DateTime.UtcNow;

        _pollRepository.UpdatePoll(poll);
        await _unitOfWork.SaveChangesAsync();

        return poll;
    }
}
