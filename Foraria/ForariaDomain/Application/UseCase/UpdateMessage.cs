using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using Foraria.Interface.DTOs.Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class UpdateMessage
    {
        private readonly IMessageRepository _repository;

        public UpdateMessage(IMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<MessageDto?> ExecuteAsync(int id, UpdateMessageRequest request)
        {
            var message = await _repository.GetById(id);
            if (message == null)
                throw new InvalidOperationException($"No se encontró el mensaje con ID {id}");

            if (!string.IsNullOrWhiteSpace(request.Content))
                message.Content = request.Content;

            if (request.FilePathToUpdate != null)
                message.optionalFile = request.FilePathToUpdate;

            if (request.RemoveFile)
                message.optionalFile = null;

            message.State = "Edited";
            await _repository.Update(message);

            return new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                State = message.State,
                OptionalFile = message.optionalFile,
                UserId = message.User_id
            };
        }
    }
}
