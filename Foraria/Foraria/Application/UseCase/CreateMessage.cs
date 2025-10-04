using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class CreateMessage
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IThreadRepository _threadRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _env;

        public CreateMessage(
            IMessageRepository messageRepository,
            IThreadRepository threadRepository,
            IUserRepository userRepository,
            IWebHostEnvironment env)
        {
            _messageRepository = messageRepository;
            _threadRepository = threadRepository;
            _userRepository = userRepository;
            _env = env;
        }

        public async Task<Message> Execute(CreateMessageWithFileRequest request)
        {
            var thread = await _threadRepository.GetById(request.Thread_id);
            if (thread == null)
                throw new InvalidOperationException($"El hilo con ID {request.Thread_id} no existe.");

            var user = await _userRepository.GetById(request.User_id);
            if (user == null)
                throw new InvalidOperationException($"El usuario con ID {request.User_id} no existe.");

            if (string.IsNullOrWhiteSpace(request.Content))
                throw new InvalidOperationException("El contenido del mensaje no puede estar vacío.");

            var message = new Message
            {
                Content = request.Content.Trim(),
                Thread_id = request.Thread_id,
                User_id = request.User_id,
                CreatedAt = DateTime.UtcNow,
                State = "active"
            };

            if (request.File != null && request.File.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Infrastructure/Storage/ForumFiles");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                message.optionalFile = Path.Combine("Infrastructure/Storage/ForumFiles", fileName);

             if (request.File.Length > 5 * 1024 * 1024)
                {
                    throw new InvalidOperationException("El archivo no puede superar los 5 MB.");

                }
            }

            return await _messageRepository.Add(message);
        }
    }
}
