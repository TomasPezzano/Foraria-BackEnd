using System.Text.Json;
using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using ForariaDomain.Models;

public class NotarizePoll
{
    private readonly IBlockchainService _blockchain;
    private readonly IBlockchainProofRepository _proofRepo;
    private readonly IUnitOfWork _uow;

    public NotarizePoll(
        IBlockchainService blockchain,
        IBlockchainProofRepository proofRepo,
        IUnitOfWork uow)
    {
        _blockchain = blockchain;
        _proofRepo = proofRepo;
        _uow = uow;
    }

    public async Task<BlockchainProof> ExecuteAsync(int pollId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("El texto de la votación no puede estar vacío.", nameof(text));

        var uri = $"app://poll/{pollId}";

        var hashBytes = _blockchain.ComputeSha256(text.Trim());
        var hashHex = _blockchain.BytesToHex(hashBytes);

        if (!hashHex.StartsWith("0x"))
            hashHex = "0x" + hashHex;

        var (txHash, _) = await _blockchain.NotarizeAsync(hashHex, uri);

        var proof = new BlockchainProof
        {
            PollId = pollId,
            HashHex = hashHex,
            Uri = uri,
            TxHash = txHash,
            Contract = _blockchain.ContractAddress,
            Network = "polygon",
            ChainId = 80002,
            CreatedAtUtc = DateTime.Now
        };

        _proofRepo.Add(proof);
        await _uow.SaveChangesAsync();

        return proof;
    }

  public string BuildNotarizableJson(PollWithResultsDomain poll)
{
    var doc = new PollNotarizableDocument
    {
        Id = poll.Id,
        Title = poll.Title,
        Description = poll.Description,
        CreatedAt = poll.CreatedAt,
        DeletedAt = poll.DeletedAt,
        State = poll.State,
        Options = poll.PollOptions.OrderBy(o => o.Id).Select(o => o.Text).ToList(),
        Results = poll.PollResults
            .OrderBy(r => r.PollOptionId)
            .Select(r => new PollNotarizableDocument.PollResultItem
            {
                OptionId = r.PollOptionId,
                VotesCount = r.VotesCount
            })
            .ToList()
    };

    return JsonSerializer.Serialize(doc, new JsonSerializerOptions
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
}


}
