using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;

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
}
