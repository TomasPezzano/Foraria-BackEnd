using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class Thread
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Theme { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public string State { get; set; }

    public int ForumId { get; set; }   

    public Forum Forum { get; set; }

    public int UserId { get; set; }

    public User User { get; set; }

    public ICollection<Message> Messages { get; set; }
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();


}
