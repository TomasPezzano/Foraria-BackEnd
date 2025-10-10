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

    public string? Description { get; set; }

    public string Category { get; set; }
    public DateTime CreatedAt { get; set; }

    public string Url { get; set; }

    public int User_id { get; set; }
    public User User { get; set; }

    public int Consortium_id { get; set; }
    public Consortium Consortium { get; set; }
}
