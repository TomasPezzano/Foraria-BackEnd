using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class CreateMessage
    {
        private readonly IMessageRepository _repository;
        private readonly IWebHostEnvironment _env;

        public CreateMessage(IMessageRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }

        public async Task<Message> Execute(CreateMessageWithFileRequest request)
        {
            var message = new Message
            {
                Content = request.Content,
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
            }

            return await _repository.Add(message);
        }
    }
}