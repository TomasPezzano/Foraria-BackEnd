using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public class CreateThread
{
    private readonly IThreadRepository _threadRepository;
    private readonly IForumRepository _forumRepository;
    private readonly IUserRepository _userRepository;

    public CreateThread(IThreadRepository threadRepository, IForumRepository forumRepository, IUserRepository userRepository)
    {
        _threadRepository = threadRepository;
        _forumRepository = forumRepository;
        _userRepository = userRepository;
    }

    public async Task<Thread> Execute(Thread thread)
    {
        var forum = await _forumRepository.GetById(thread.ForumId);
        if (forum == null)
            throw new InvalidOperationException($"El foro con ID {thread.ForumId} no existe.");

        var user = await _userRepository.GetById(thread.UserId);
        if (user == null)
            throw new InvalidOperationException($"El usuario con ID {thread.UserId} no existe.");

        var existingThread = forum.Threads.FirstOrDefault(t =>
            t.Theme.Trim().ToLower() == thread.Theme.Trim().ToLower());

        if (existingThread != null)
            throw new InvalidOperationException($"Ya existe un hilo con el título '{thread.Theme}' en este foro.");

        await _threadRepository.Add(thread);

        return thread;
    }
}