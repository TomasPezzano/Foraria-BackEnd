using ForariaDomain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Call
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int CreatedByUserId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string MeetingType { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Status { get; set; } = "Active";

    public int? ConsortiumId { get; set; }
    public Consortium? Consortium { get; set; }
}
