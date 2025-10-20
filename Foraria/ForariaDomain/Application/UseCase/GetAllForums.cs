using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;


namespace Foraria.Application.UseCase
{
    public class GetAllForums
    {
        private readonly IForumRepository _repository;

        public GetAllForums(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ForumResponse>> Execute()
        {
            var forums = await _repository.GetAll();

            var activeForums = forums.Where(f => f.IsActive);

            return activeForums.Select(f => new ForumResponse
            {
                Id = f.Id,
                Category = f.Category,
                CategoryName = f.Category.ToString()
            });
        }
    }
}
