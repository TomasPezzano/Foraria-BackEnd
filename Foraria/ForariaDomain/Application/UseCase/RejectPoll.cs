using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;

public class RejectPoll
{
    private readonly IPollRepository _pollRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectPoll(IPollRepository pollRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _pollRepository = pollRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Poll> ExecuteAsync(int pollId, int approverUserId)
    {
        var poll = await _pollRepository.GetById(pollId)
            ?? throw new NotFoundException($"La votación con ID {pollId} no existe.");

        if (poll.State != "Pendiente")
            throw new InvalidOperationException("Solo pueden rechazarse votaciones en estado Pendiente.");

        var user = await _userRepository.GetById(approverUserId)
            ?? throw new NotFoundException($"El usuario con ID {approverUserId} no existe.");

        if (user.Role.Description != "Consorcio")
            throw new UnauthorizedAccessException("Solo los usuarios con rol Consorcio pueden aprobar votaciones.");

        poll.State = "Rechazada";
        poll.ApprovedAt = DateTime.UtcNow;
        poll.ApprovedByUserId = user.Id;

        _pollRepository.Update(poll);
        await _unitOfWork.SaveChangesAsync();

        return poll;
    }
}
