using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using Thread = ForariaDomain.Thread;

namespace Foraria.Application.UseCase
{
    public class UpdateThread
    {
        private readonly IThreadRepository _threadRepository;
        private readonly IUserRepository _userRepository;

        public UpdateThread(IThreadRepository threadRepository, IUserRepository userRepository)
        {
            _threadRepository = threadRepository;
            _userRepository = userRepository;
        }

        public async Task<Thread> ExecuteAsync(int threadId, UpdateThreadRequest request)
        {
            var user = await _userRepository.GetById(request.UserId)
                ?? throw new NotFoundException($"No se encontró el usuario con id {request.UserId}");

            var thread = await _threadRepository.GetById(threadId)
                ?? throw new NotFoundException($"No se encontró el hilo con id {threadId}");

            var isOwner = thread.UserId == user.Id;
            var roleName = user.Role.Description.ToLower();
            var isAdminOrConsortium = roleName == "administrador" || roleName == "consorcio";

            if (!isOwner && !isAdminOrConsortium)
                throw new ForbiddenAccessException("No tienes permisos para editar este hilo.");

            if (!isAdminOrConsortium && thread.State is "Closed" or "Archived")
                throw new ForbiddenAccessException("No puedes modificar un hilo cerrado o archivado.");

            if (!string.IsNullOrWhiteSpace(request.Theme))
                thread.Theme = request.Theme;

            if (!string.IsNullOrWhiteSpace(request.Description))
                thread.Description = request.Description;

            if (!string.IsNullOrWhiteSpace(request.State))
            {
                if (!isAdminOrConsortium)
                    throw new ForbiddenAccessException("No tienes permisos para cambiar el estado del hilo.");

                var current = thread.State;
                var target = request.State;

                if (current == "Archived" && target != "Archived")
                    throw new ForbiddenAccessException("No se puede modificar el estado de un hilo archivado.");

                if (current == "Closed" && target == "Active")
                    throw new ForbiddenAccessException("No se puede reabrir un hilo cerrado.");

                thread.State = target;
            }

            thread.UpdatedAt = DateTime.UtcNow;
            await _threadRepository.UpdateAsync(thread);
            return thread;
        }
    }
}