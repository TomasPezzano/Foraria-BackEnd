using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaDomain.Application.UseCase;

public class UpdateThread
{
    private readonly IThreadRepository _threadRepository;
    private readonly IUserRepository _userRepository;

    public UpdateThread(IThreadRepository threadRepository, IUserRepository userRepository)
    {
        _threadRepository = threadRepository;
        _userRepository = userRepository;
    }

    public async Task<Thread> ExecuteAsync(int threadId, Thread threadd)
    {
        var user = await _userRepository.GetById(threadd.UserId)
            ?? throw new NotFoundException($"No se encontró el usuario con id {threadd.UserId}");

        var thread = await _threadRepository.GetById(threadId)
            ?? throw new NotFoundException($"No se encontró el hilo con id {threadId}");

        var isOwner = thread.UserId == user.Id;
        var roleName = user.Role.Description.ToLower();
        var isAdminOrConsortium = roleName == "administrador" || roleName == "consorcio";

        if (!isOwner && !isAdminOrConsortium)
            throw new ForbiddenAccessException("No tienes permisos para editar este hilo.");

        if (!isAdminOrConsortium && thread.State is "Closed" or "Archived")
            throw new ForbiddenAccessException("No puedes modificar un hilo cerrado o archivado.");

        if (!string.IsNullOrWhiteSpace(threadd.Theme))
            thread.Theme = threadd.Theme;

        if (!string.IsNullOrWhiteSpace(threadd.Description))
            thread.Description = threadd.Description;

        if (!string.IsNullOrWhiteSpace(threadd.State))
        {
            if (!isAdminOrConsortium)
                throw new ForbiddenAccessException("No tienes permisos para cambiar el estado del hilo.");

            var current = thread.State;
            var target = threadd.State;

            if (current == "Archived" && target != "Archived")
                throw new ForbiddenAccessException("No se puede modificar el estado de un hilo archivado.");

            if (current == "Closed" && target == "Active")
                throw new ForbiddenAccessException("No se puede reabrir un hilo cerrado.");

            thread.State = target;
        }

        thread.UpdatedAt = DateTime.Now;
        await _threadRepository.UpdateAsync(thread);
        return thread;
    }
}