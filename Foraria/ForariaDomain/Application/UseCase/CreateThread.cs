using Foraria.Interface.DTOs;
using Foraria.Domain.Repository;
using ForariaDomain;
using System;
using System.Threading.Tasks;
using Thread = ForariaDomain.Thread;

namespace Foraria.Application.UseCase
{
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

        public async Task<ThreadResponse> Execute(CreateThreadRequest request)
        {
            var forum = await _forumRepository.GetById(request.ForumId);
            if (forum == null)
                throw new InvalidOperationException($"El foro con ID {request.ForumId} no existe.");

            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
                throw new InvalidOperationException($"El usuario con ID {request.UserId} no existe.");

            var existingThread = forum.Threads.FirstOrDefault(t =>
                t.Theme.Trim().ToLower() == request.Theme.Trim().ToLower());

            if (existingThread != null)
                throw new InvalidOperationException($"Ya existe un hilo con el título '{request.Theme}' en este foro.");

            var thread = new Thread
            {
                Theme = request.Theme.Trim(),
                Description = request.Description.Trim(),
                ForumId = request.ForumId,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                State = "Active"
            };

            await _threadRepository.Add(thread);

            return new ThreadResponse
            {
                Id = thread.Id,
                Theme = thread.Theme,
                Description = thread.Description,
                CreatedAt = thread.CreatedAt,
                State = thread.State,
                Forum_id = thread.ForumId,
                User_id = thread.UserId
            };
        }
    }
}