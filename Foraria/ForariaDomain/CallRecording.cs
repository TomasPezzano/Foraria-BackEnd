using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain
{
    public class CallRecording
    {
        public int Id { get; set; }
        public int CallId { get; set; }
        public string FilePath { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
