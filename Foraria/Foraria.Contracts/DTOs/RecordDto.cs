using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace Foraria.Infrastructure.Blockchain
{
    [FunctionOutput]
    public class RecordDto
    {
        [Parameter("string", "uri", 1)]
        public string Uri { get; set; }

        [Parameter("address", "who", 2)]
        public string Who { get; set; }

        [Parameter("uint256", "timestamp", 3)]
        public BigInteger Timestamp { get; set; }
    }
}
