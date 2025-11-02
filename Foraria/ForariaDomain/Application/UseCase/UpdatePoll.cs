using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Exceptions;

namespace Foraria.Application.UseCase
{
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

        public async Task<Poll> ExecuteAsync(int pollId, UpdatePollRequest request)
        {
            var poll = await _pollRepository.GetById(pollId)
                ?? throw new NotFoundException($"La votación con ID {pollId} no existe.");

            var user = await _userRepository.GetById(request.UserId)
                ?? throw new NotFoundException($"El usuario con ID {request.UserId} no existe.");

            bool isOwner = poll.User_id == request.UserId;
            bool isConsortium = user.Role.Description == "Consorcio";

            if (!isOwner && !isConsortium)
                throw new UnauthorizedAccessException("No tiene permisos para modificar esta votación.");

            if (poll.State == "Active" || poll.State == "Closed" || poll.State == "Rejected")
                throw new InvalidOperationException("No se pueden modificar votaciones activas, cerradas o rechazadas.");

            if (!string.IsNullOrWhiteSpace(request.Title))
                poll.Title = request.Title;

            if (!string.IsNullOrWhiteSpace(request.Description))
                poll.Description = request.Description;

            if (request.StartDate.HasValue)
                poll.StartDate = request.StartDate.Value;

            if (request.EndDate.HasValue)
                poll.EndDate = request.EndDate.Value;

            if (!string.IsNullOrWhiteSpace(request.State))
                poll.State = request.State;

            await _pollRepository.UpdatePoll(poll);
            await _unitOfWork.SaveChangesAsync();

            return poll;
        }
    }
}
