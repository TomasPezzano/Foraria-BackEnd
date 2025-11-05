using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Models;

public class ForumWithStats
{
    public int Id { get; set; }
    public ForumCategory Category { get; set; }
    public int CountThreads { get; set; }
    public int CountResponses { get; set; }
    public int CountUserActives { get; set; }
}
