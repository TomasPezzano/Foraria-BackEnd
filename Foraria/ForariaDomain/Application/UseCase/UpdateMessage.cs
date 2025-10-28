using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase
{
    public class UpdateMessage
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public UpdateMessage(IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task<Message> ExecuteAsync(int messageId, UpdateMessageRequest request)
        {
            var user = await _userRepository.GetById(request.UserId)
                ?? throw new NotFoundException($"No se encontró el usuario con id {request.UserId}");

            var message = await _messageRepository.GetById(messageId)
                ?? throw new NotFoundException($"No se encontró el mensaje con id {messageId}");

            if (message.IsDeleted)
                throw new InvalidOperationException("No se puede editar un mensaje eliminado.");

            var isOwner = message.User_id == user.Id;
            var roleName = user.Role.Description.ToLower();
            var isAdminOrConsortium = roleName == "admin" || roleName == "consorcio";

            var minutesSinceCreation = (DateTime.UtcNow - message.CreatedAt).TotalMinutes;
            if (isOwner && !isAdminOrConsortium && minutesSinceCreation > 15)
                throw new ForbiddenAccessException("Solo puedes editar tu mensaje dentro de los primeros 15 minutos.");

            if (!isOwner && !isAdminOrConsortium)
                throw new ForbiddenAccessException("No tienes permisos para editar este mensaje.");

            if (!string.IsNullOrWhiteSpace(request.Content))
                message.Content = request.Content;

            if (!string.IsNullOrEmpty(request.FilePathToUpdate))
                message.optionalFile = request.FilePathToUpdate;

            message.UpdatedAt = DateTime.UtcNow;
            await _messageRepository.Update(message);

            return message;
        }
    }
}
