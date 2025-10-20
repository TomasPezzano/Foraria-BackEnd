using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Exceptions
{
    public class ThreadNotFoundException : Exception
    {
        public ThreadNotFoundException(string message) : base(message) { }
    }
}
