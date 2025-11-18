namespace Foraria.DTOs
{
    public class ThreadWithMessagesDto
    {
        public int Id { get; set; }
        public string Theme { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string State { get; set; }
        public int UserId { get; set; }
        public int ForumId { get; set; }
        public List<MessageDto> Messages { get; set; } = new();
    }
}