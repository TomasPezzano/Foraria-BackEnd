using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ForariaDomain;

public class Forum
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
    public ForumCategory Category { get; set; }

    public ICollection<Thread> Threads { get; set; } = new List<Thread>();

    public int? ConsortiumId { get; set; }

    public Consortium? Consortium { get; set; }
}
