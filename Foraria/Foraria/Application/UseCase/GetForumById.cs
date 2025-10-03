using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class GetForumById
    {
        private readonly IForumRepository _repository;

        public GetForumById(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task<ForumResponse?> Execute(int id)
        {
            var forum = await _repository.GetById(id);
            if (forum == null) return null;

            return new ForumResponse
            {
                Id = forum.Id,
                Category = forum.Category
            };
        }
    }
}
