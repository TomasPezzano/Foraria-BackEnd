using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForariaDomain;

public class Vote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Poll Poll { get; set; }

    public int Poll_id { get; set; }

    public User User { get; set; }

    public int User_id { get; set; }

    public PollOption PollOption { get; set; }  

    public int PollOption_id { get; set; }

    public DateTime VotedDate { get; set; }


}