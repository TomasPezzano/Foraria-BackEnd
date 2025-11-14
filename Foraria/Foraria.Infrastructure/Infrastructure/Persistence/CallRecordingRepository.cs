using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Infrastructure.Infrastructure.Persistence
{
    using ForariaDomain;
    using ForariaDomain.Repository;
    using ForariaDomain.Repository.ForariaDomain.Repository;
    using global::Foraria.Infrastructure.Persistence;
    using Microsoft.EntityFrameworkCore;

    namespace Foraria.Infrastructure.Persistence
    {
        public class CallRecordingRepository : ICallRecordingRepository
        {
            private readonly ForariaContext _context;

            public CallRecordingRepository(ForariaContext context)
            {
                _context = context;
            }

            public void Save(CallRecording recording)
            {
                _context.CallRecordings.Add(recording);
                _context.SaveChanges();
            }

            public List<CallRecording> GetByCall(int callId)
            {
                return _context.CallRecordings
                    .Where(r => r.CallId == callId)
                    .AsNoTracking()
                    .ToList();
            }
        }
    }
}
