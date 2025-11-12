using System.Security.Cryptography;


namespace ForariaDomain.Application.UseCase;

public interface IPasswordResetTokenGenerator
{
    string Generate();
}

public class PasswordResetTokenGenerator : IPasswordResetTokenGenerator
{
    public string Generate()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
