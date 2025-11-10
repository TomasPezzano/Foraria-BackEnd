using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using ForariaDomain.Exceptions;

namespace ForariaDomain.Application.UseCase;

public class VerifyFileProof
{
    private readonly IBlockchainService _blockchain;
    private readonly IBlockchainProofRepository _proofRepo;

    public VerifyFileProof(IBlockchainService blockchain, IBlockchainProofRepository proofRepo)
    {
        _blockchain = blockchain;
        _proofRepo = proofRepo;
    }

    public async Task<bool> ExecuteAsync(Guid documentId, string filePath)
    {
        if (!File.Exists(filePath))
            throw new NotFoundException("El archivo especificado no fue encontrado.");

        var proof = await _proofRepo.GetByDocumentIdAsync(documentId);
        if (proof == null)
            throw new NotFoundException($"No se encontró una prueba registrada para el documento {documentId}.");

        try
        {
            var isValid = await _blockchain.VerifyFileAsync(filePath, proof.HashHex);
            return isValid;
        }
        catch (Exception ex)
        {
            throw new BlockchainException($"Error al verificar la prueba de blockchain: {ex.Message}");
        }
    }
}
