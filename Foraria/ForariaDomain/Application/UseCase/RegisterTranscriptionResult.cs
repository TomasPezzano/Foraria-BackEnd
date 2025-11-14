using ForariaDomain;
using ForariaDomain.Repository;

public class RegisterTranscriptionResult
{
    private readonly ICallTranscriptRepository _transcriptRepo;

    public RegisterTranscriptionResult(ICallTranscriptRepository transcriptRepo)
    {
        _transcriptRepo = transcriptRepo;
    }

    public CallTranscript Execute(
        int callId,
        string transcriptPath,
        string? audioPath,
        string transcriptHash,
        string? audioHash)
    {
        var existing = _transcriptRepo.GetByCallId(callId);

        if (existing != null)
        {
            existing.TranscriptPath = transcriptPath;
            existing.AudioPath = audioPath;
            existing.TranscriptHash = transcriptHash;
            existing.AudioHash = audioHash;
            _transcriptRepo.Update(existing);
            return existing;
        }

        var transcript = new CallTranscript
        {
            CallId = callId,
            TranscriptPath = transcriptPath,
            AudioPath = audioPath,
            TranscriptHash = transcriptHash,
            AudioHash = audioHash
        };

        return _transcriptRepo.Create(transcript);
    }
}
