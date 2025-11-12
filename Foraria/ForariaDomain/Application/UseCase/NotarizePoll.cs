using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using System;

namespace ForariaDomain.Application.UseCase;

public class NotarizePoll
{
    private readonly IBlockchainService _blockchain;
    private readonly IBlockchainProofRepository _proofRepo;

    public NotarizePoll(IBlockchainService blockchain, IBlockchainProofRepository proofRepo)
    {
        _blockchain = blockchain;
        _proofRepo = proofRepo;
    }

    public async Task<BlockchainProof> ExecuteAsync(int pollId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("El texto de la votación no puede estar vacío.", nameof(text));

        var uri = $"app://poll/{pollId}";

        var hashBytes = _blockchain.ComputeSha256(text.Trim());
        var hashHex = _blockchain.BytesToHex(hashBytes);

        if (!hashHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hashHex = "0x" + hashHex;

        var (txHash, _) = await _blockchain.NotarizeAsync(hashHex, uri);

        var proof = new BlockchainProof
        {
            PollId = pollId,
            HashHex = hashHex,
            Uri = uri,
            TxHash = txHash,
            Contract = "0xA8a7BcAb69C858929e03f5f93986F65195aE935C",
            Network = "polygon",
            ChainId = 80002,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _proofRepo.AddAsync(proof);
        return proof;
    }
}
