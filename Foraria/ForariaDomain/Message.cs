using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class Message
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Content { get; set; }

    public DateTime CreatedAt { get; set; }

    public string State { get; set; }

    public string optionalFile { get; set; }

    public int Thread_id { get; set; }

    public Thread Thread { get; set; }

    public int User_id { get; set; }

    public User User { get; set; }


}
