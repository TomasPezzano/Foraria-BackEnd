namespace Foraria.Interface.DTOs
{
    public class PollDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryPollId { get; set; }
        public int UserId { get; set; }
        public List<string> Options { get; set; }
    }
}
