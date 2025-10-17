namespace Foraria.Domain.Service
{
    public interface IBlockchainService
    {
        Task<(string TxHash, string HashHex)> NotarizeAsync(string text, string uri);
        Task<bool> VerifyAsync(string text, string expectedHashHex);
        Task<bool> VerifyFileAsync(string filePath, string expectedHashHex);
        byte[] ComputeSha256FromFile(string filePath);
        string BytesToHex(byte[] bytes);
        byte[] ComputeSha256(string input);

    }
}
