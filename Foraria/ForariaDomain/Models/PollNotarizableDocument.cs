using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Models
{
    public class PollNotarizableDocument
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string State { get; set; } = null!;
        public List<string> Options { get; set; } = new();
        public List<PollResultItem> Results { get; set; } = new();

        public class PollResultItem
        {
            public int OptionId { get; set; }
            public int VotesCount { get; set; }
        }
    }
}
