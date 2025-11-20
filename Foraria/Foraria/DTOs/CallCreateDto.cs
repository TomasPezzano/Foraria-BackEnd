using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class CallCreateDto
{
    public int UserId { get; set; }

    public string Title { get; set; }
    public string? Description { get; set; }
    public string MeetingType { get; set; }
    public int ConsortiumId { get; set; }

}
