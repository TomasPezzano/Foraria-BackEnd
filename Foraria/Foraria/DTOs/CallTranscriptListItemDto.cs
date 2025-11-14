namespace Foraria.DTOs;

public class CallTranscriptListItemDto
{
    public int Id { get; set; }
    public int CallId { get; set; }
    public string? BlockchainTxHash { get; set; }
    public string BlockchainExplorerUrl { get; set; } = string.Empty;
}