using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase
{
    public class DeleteMessage
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public DeleteMessage(IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync(int messageId, int requestingUserId)
        {
            var message = await _messageRepository.GetById(messageId);
            if (message == null)
                throw new NotFoundException($"No se encontró el mensaje con id {messageId}");

            var user = await _userRepository.GetById(requestingUserId);
            if (user == null)
                throw new NotFoundException($"No se encontró el usuario con id {requestingUserId}");

            var isOwner = message.User_id == user.Id;
            var roleName = user.Role.Description.ToLower();

            var isAdminOrConsortium = roleName == "Administrador" || roleName == "Consorcio";

            if (!isOwner && !isAdminOrConsortium)
                throw new ForbiddenAccessException("No tienes permisos para eliminar este mensaje.");

            if (message.IsDeleted)
                return;

            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;

            await _messageRepository.Update(message);
        }
    }
}