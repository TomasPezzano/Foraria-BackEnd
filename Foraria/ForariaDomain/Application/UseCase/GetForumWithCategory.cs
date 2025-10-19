using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase
{
    public class GetForumWithCategory
    {
        private readonly IForumRepository _repository;

        public GetForumWithCategory(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task<ForumWithCategoryResponse?> Execute(int id)
        {
            var forum = await _repository.GetById(id);
            if (forum == null)
                return null;

            return new ForumWithCategoryResponse
            {
                Id = forum.Id,
                CategoryName = forum.Category.ToString(),
                CategoryValue = (int)forum.Category,
                ThreadCount = forum.Threads?.Count ?? 0
            };
        }
    }
}
