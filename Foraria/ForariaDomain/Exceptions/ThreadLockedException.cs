using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Exceptions
{
    public class ThreadLockedException : Exception
    {
        public ThreadLockedException(string message) : base(message) { }
    }

}
