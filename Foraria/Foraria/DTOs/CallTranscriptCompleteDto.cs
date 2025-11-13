using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class CallTranscriptCompleteDto
{
    [Required]
    public string TranscriptPath { get; set; }

    public string? AudioPath { get; set; }
}
