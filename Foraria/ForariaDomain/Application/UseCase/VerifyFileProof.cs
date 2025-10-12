using Foraria.Domain.Repository;
using Foraria.Domain.Service;

namespace Foraria.Application.UseCase
{
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
            var proof = await _proofRepo.GetByDocumentIdAsync(documentId);
            if (proof == null)
                throw new InvalidOperationException($"No se encontró una prueba para el documento {documentId}");

            return await _blockchain.VerifyFileAsync(filePath, proof.HashHex);
        }
    }
}
