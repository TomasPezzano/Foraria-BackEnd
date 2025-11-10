using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;


namespace ForariaDomain.Application.UseCase;

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

    public async Task<Message> Execute(Message message)
    {
        var thread = await _threadRepository.GetById(message.Thread_id)
            ?? throw new InvalidOperationException($"El hilo con ID {message.Thread_id} no existe.");

        var user = await _userRepository.GetById(message.User_id)
            ?? throw new InvalidOperationException($"El usuario con ID {message.User_id} no existe.");

        if (string.IsNullOrWhiteSpace(message.Content))
            throw new InvalidOperationException("El contenido del mensaje no puede estar vacío.");

        return await _messageRepository.Add(message);
    }
}
