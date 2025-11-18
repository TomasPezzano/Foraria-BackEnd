using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaDomain.Application.UseCase;

public class UpdateMessage
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;

    public UpdateMessage(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    public async Task<Message> ExecuteAsync(Message editedMessage, int userId)
    {
        var user = await _userRepository.GetById(userId)
              ?? throw new NotFoundException($"No se encontró el usuario con id {userId}");

        var existingMessage = await _messageRepository.GetById(editedMessage.Id)
            ?? throw new NotFoundException($"No se encontró el mensaje con id {editedMessage.Id}");

        if (existingMessage.IsDeleted)
            throw new InvalidOperationException("No se puede editar un mensaje eliminado.");

        var isOwner = existingMessage.User_id == user.Id;
        var roleName = user.Role.Description.ToLower();
        var isAdminOrConsortium = roleName == "admin" || roleName == "consorcio";

        var minutesSinceCreation = (DateTime.Now - existingMessage.CreatedAt).TotalMinutes;

        if (isOwner && !isAdminOrConsortium && minutesSinceCreation > 15)
            throw new ForbiddenAccessException("Solo puedes editar tu mensaje dentro de los primeros 15 minutos.");

        if (!isOwner && !isAdminOrConsortium)
            throw new ForbiddenAccessException("No tienes permisos para editar este mensaje.");

 
        if (!string.IsNullOrWhiteSpace(editedMessage.Content))
            existingMessage.Content = editedMessage.Content;

        if (!string.IsNullOrEmpty(editedMessage.optionalFile))
            existingMessage.optionalFile = editedMessage.optionalFile;

        existingMessage.UpdatedAt = DateTime.Now;

        await _messageRepository.Update(existingMessage);

        return existingMessage;
    }
}
