using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class CallTranscriptCompleteDto
{
    public string TranscriptPath { get; set; }
    public string? AudioPath { get; set; }

    public string TranscriptHash { get; set; }
    public string? AudioHash { get; set; }
}
