using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace ForariaDomain.Models;

[FunctionOutput]
public class RecordResult
{
    [Parameter("string", "uri", 1)]
    public string Uri { get; set; }

    [Parameter("address", "who", 2)]
    public string Who { get; set; }

    [Parameter("uint256", "timestamp", 3)]
    public BigInteger Timestamp { get; set; }
}
