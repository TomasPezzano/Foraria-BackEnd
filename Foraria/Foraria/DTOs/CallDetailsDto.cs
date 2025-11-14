namespace Foraria.DTOs
{
    public class CallDetailsDto
    {
        public int Id { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime StartedAt { get; set; }
        public string Status { get; set; } = default!;
    }
}
