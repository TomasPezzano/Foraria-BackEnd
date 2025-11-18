using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForariaDomain;

public class CallTranscript
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int CallId { get; set; }

    public string TranscriptPath { get; set; } = string.Empty;
    public string? AudioPath { get; set; }

    public string TranscriptHash { get; set; } = string.Empty;
    public string? AudioHash { get; set; }

    public string? BlockchainTxHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
