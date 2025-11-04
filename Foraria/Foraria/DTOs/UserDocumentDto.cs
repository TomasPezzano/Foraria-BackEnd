namespace Foraria.DTOs;

public class UserDocumentDto
{
    public int Id { get; set; }

    public string Title { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Url { get; set; }
    public int User_id { get; set; }
    public int Consortium_id { get; set; }
}
