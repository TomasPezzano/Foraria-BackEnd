using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository
{
    public interface ICallMessageRepository
    {
        void Save(CallMessage message);
        List<CallMessage> GetLastByCall(int callId, int limit = 50);
    }
}
