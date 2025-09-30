using Foraria.Domain.Repository;
using ForariaDomain;
using System;
using System.Threading.Tasks;
using Thread = ForariaDomain.Thread;

namespace Foraria.Application.UseCase
{
    public class CreateForum
    {
        private readonly IForumRepository _repository;

        public CreateForum(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task<Forum> Execute(Forum forum)
        {
            forum.Threads = forum.Threads ?? new List<Thread>();
            return await _repository.Add(forum);
        }
    }
}