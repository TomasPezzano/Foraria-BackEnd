namespace Foraria.Domain.Service
{
    public interface IBlockchainService
    {
        Task<(string TxHash, string HashHex)> NotarizeAsync(string text, string uri);
    }
}
