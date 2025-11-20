using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Models
{
    public class PollWithResultsDomain
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        public string State { get; set; }
        public int CategoryPollId { get; set; }
        public List<PollOptionDomain> PollOptions { get; set; } = new List<PollOptionDomain>();
        public List<PollResult> PollResults { get; set; } = new List<PollResult>();
    }
}
