using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;

public class NotarizeFile
{
    private readonly IBlockchainService _blockchain;
    private readonly IBlockchainProofRepository _proofRepo;
    private readonly IUnitOfWork _uow;

    public NotarizeFile(
        IBlockchainService blockchain,
        IBlockchainProofRepository proofRepo,
        IUnitOfWork uow)
    {
        _blockchain = blockchain;
        _proofRepo = proofRepo;
        _uow = uow;
    }

    public async Task<BlockchainProof> ExecuteAsync(Guid documentId, string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("No se encontró el archivo a notarizar.", filePath);

        var hashBytes = _blockchain.ComputeSha256FromFile(filePath);
        var hashHex = _blockchain.BytesToHex(hashBytes);

        var uri = $"app://document/{documentId}";
        var (txHash, _) = await _blockchain.NotarizeAsync(hashHex, uri);

        var proof = new BlockchainProof
        {
            DocumentId = documentId,
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
