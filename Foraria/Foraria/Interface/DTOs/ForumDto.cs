using Foraria.Domain.Model;

namespace Foraria.Interface.DTOs
{
    public class CreateForumRequest
    {
        public ForumCategory Category { get; set; }
    }

    public class ForumResponse
    {
        public int Id { get; set; }
        public ForumCategory Category { get; set; }
    }
}