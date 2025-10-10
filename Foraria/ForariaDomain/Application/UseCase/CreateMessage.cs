using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

public class CreateMessage
{
    private readonly IMessageRepository _messageRepository;
    private readonly IThreadRepository _threadRepository;
    private readonly IUserRepository _userRepository;

    public CreateMessage(
        IMessageRepository messageRepository,
        IThreadRepository threadRepository,
        IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _threadRepository = threadRepository;
        _userRepository = userRepository;
    }

    public async Task<Message> Execute(CreateMessageWithFileRequest request)
    {
        var thread = await _threadRepository.GetById(request.Thread_id)
            ?? throw new InvalidOperationException($"El hilo con ID {request.Thread_id} no existe.");

        var user = await _userRepository.GetById(request.User_id)
            ?? throw new InvalidOperationException($"El usuario con ID {request.User_id} no existe.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new InvalidOperationException("El contenido del mensaje no puede estar vacío.");

        var message = new Message
        {
            Content = request.Content.Trim(),
            Thread_id = request.Thread_id,
            User_id = request.User_id,
            CreatedAt = DateTime.UtcNow,
            State = "active",
            optionalFile = request.FilePath
        };

        return await _messageRepository.Add(message);
    }
}
