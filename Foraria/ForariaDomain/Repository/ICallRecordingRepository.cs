using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository
{
    namespace ForariaDomain.Repository
    {
        public interface ICallRecordingRepository
        {
            void Save(CallRecording recording);
            List<CallRecording> GetByCall(int callId);
        }
    }
}
