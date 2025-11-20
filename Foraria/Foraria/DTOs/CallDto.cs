namespace Foraria.DTOs;

public class CallDto
{
    public int Id { get; set; }
    public int CreatedByUserId { get; set; }

    public string Title { get; set; }
    public string? Description { get; set; }
    public string MeetingType { get; set; }

    public DateTime StartedAt { get; set; }
    public string Status { get; set; }
}
