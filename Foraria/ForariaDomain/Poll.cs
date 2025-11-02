using Foraria.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class Poll
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime DeletedAt { get; set; }

    public string State { get; set; }

    public User User { get; set; }

    public int User_id { get; set; }

    public CategoryPoll CategoryPoll { get; set; }

    public int CategoryPoll_id { get; set; }

    public ICollection<PollOption> PollOptions { get; set; }

    public ResultPoll ResultPoll { get; set; }

    public int ? ResultPoll_id { get; set; }

    public ICollection<Vote> Votes { get; set; }
    public BlockchainProof? BlockchainProof { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
