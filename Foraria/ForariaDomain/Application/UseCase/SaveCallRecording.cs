using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using ForariaDomain.Repository.ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase
{
    public class SaveCallRecording
    {
        private readonly ICallRepository _callRepository;
        private readonly ICallRecordingRepository _recordingRepository;

        public SaveCallRecording(
            ICallRepository callRepository,
            ICallRecordingRepository recordingRepository)
        {
            _callRepository = callRepository;
            _recordingRepository = recordingRepository;
        }

        public void Execute(int callId, string filePath, string contentType)
        {
            var call = _callRepository.GetById(callId);
            if (call == null)
                throw new NotFoundException("La llamada no existe.");

            var recording = new CallRecording
            {
                CallId = callId,
                FilePath = filePath,
                ContentType = contentType
            };

            _recordingRepository.Save(recording);
        }
    }
}
