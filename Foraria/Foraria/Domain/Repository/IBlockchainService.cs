using System.Threading.Tasks;

namespace Foraria.Domain.Repository
{
    public interface IBlockchainService
    {
        Task<(string txHash, string hashHex)> NotarizeAsync(string text, string uri);
    }
}