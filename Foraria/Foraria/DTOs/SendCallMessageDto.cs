namespace Foraria.DTOs
{
    public class SendCallMessageDto
    {
        public int UserId { get; set; }
        public string Message { get; set; } = default!;
    }
}
