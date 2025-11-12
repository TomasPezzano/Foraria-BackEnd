using System.Security.Cryptography;

namespace ForariaDomain.Application.UseCase;

public interface IRefreshTokenGenerator
{
    string Generate();
}

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string Generate()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
