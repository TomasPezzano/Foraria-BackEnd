using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain
{
    public class CallMessage
    {
        public int Id { get; set; }
        public int CallId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = default!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }

}
