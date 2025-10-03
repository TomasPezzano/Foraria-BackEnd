using Foraria.Interface.DTOs;
using Foraria.Domain.Repository;
using ForariaDomain;
using System.Threading.Tasks;

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