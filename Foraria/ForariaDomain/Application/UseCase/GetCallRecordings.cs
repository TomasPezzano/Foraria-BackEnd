using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase
{
    using ForariaDomain.Exceptions;
    using ForariaDomain.Exceptions;
    using ForariaDomain.Repository;
    using ForariaDomain.Repository.ForariaDomain.Repository;

    namespace Foraria.Application.UseCase
    {
        public class GetCallRecordings
        {
            private readonly ICallRepository _callRepository;
            private readonly ICallRecordingRepository _recordingRepository;

            public GetCallRecordings(
                ICallRepository callRepository,
                ICallRecordingRepository recordingRepository)
            {
                _callRepository = callRepository;
                _recordingRepository = recordingRepository;
            }

            public List<CallRecording> Execute(int callId)
            {
                var call = _callRepository.GetById(callId);
                if (call == null)
                    throw new NotFoundException("La llamada no existe.");

                return _recordingRepository.GetByCall(callId);
            }
        }
    }

}
