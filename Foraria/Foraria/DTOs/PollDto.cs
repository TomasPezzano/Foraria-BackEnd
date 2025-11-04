namespace Foraria.DTOs
{
    public class PollDto
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryPollId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        public string State { get; set; }

        public int UserId { get; set; }
        public List<string> Options { get; set; }
    }
}
