using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForariaDomain;

public class PollOption
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Poll Poll { get; set; }  

    public int Poll_id { get; set; }

    public string Text { get; set; }

    public ICollection<Vote> Votes { get; set; }

}