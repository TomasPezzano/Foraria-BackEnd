namespace Foraria.DTOs
{
    public class ChatMessageDto
    {
        public int UserId { get; set; }
        public string Message { get; set; } = default!;
        public DateTime SentAt { get; set; }
    }
}
