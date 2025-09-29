using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class UserDocument
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Title { get; set; }

    public string Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public string Url { get; set; }

    public int User_id { get; set; }
    public User User { get; set; }

    public int Residence_id { get; set; }
    public Residence Residence { get; set; }
}
