using Foraria.Interface.DTOs;
using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase
{
    public class CreateForum
    {
        private readonly IForumRepository _repository;

        public CreateForum(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task<ForumResponse> Execute(CreateForumRequest request)
        {
            var existingForum = await _repository.GetByCategory(request.Category);
            if (existingForum != null)
                throw new InvalidOperationException($"Ya existe un foro para la categoría '{request.Category}'.");

            var forum = new Forum
            {
                Category = request.Category
            };

            var createdForum = await _repository.Add(forum);

            return new ForumResponse
            {
                Id = createdForum.Id,
                Category = createdForum.Category
            };
        }
    }
}