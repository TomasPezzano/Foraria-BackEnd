using ForariaDomain;
using ForariaDomain.Repository;

public class RegisterTranscriptionResult
{
    private readonly ICallTranscriptRepository _transcriptRepo;

    public RegisterTranscriptionResult(
        ICallTranscriptRepository transcriptRepo)
    {
        _transcriptRepo = transcriptRepo;
    }

    public CallTranscript Execute(
        int callId,
        string transcriptPath,
        string? audioPath)
    {
        var transcript = new CallTranscript
        {
            CallId = callId,
            TranscriptPath = transcriptPath,
            AudioPath = audioPath
        };

        return _transcriptRepo.Create(transcript);
    }
}
