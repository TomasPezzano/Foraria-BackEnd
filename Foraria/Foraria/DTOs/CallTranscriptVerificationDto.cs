namespace Foraria.DTOs;

public class CallTranscriptVerificationDto
{
    public int CallId { get; set; }
    public bool ExistsOnChain { get; set; }
    public bool HashMatches { get; set; }

    public string TranscriptHash { get; set; } = string.Empty;
    public string? AudioHash { get; set; }
    public string BlockchainTxHash { get; set; } = string.Empty;

    public string BlockchainExplorerUrl { get; set; } = string.Empty;

    public string TranscriptPath { get; set; } = string.Empty;
    public string? AudioPath { get; set; }
}