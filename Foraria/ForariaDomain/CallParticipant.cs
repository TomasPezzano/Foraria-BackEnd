using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForariaDomain;

public class CallParticipant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int CallId { get; set; }
    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}
