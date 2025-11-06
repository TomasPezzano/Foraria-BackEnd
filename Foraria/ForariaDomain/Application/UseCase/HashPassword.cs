using BCrypt.Net;

namespace Foraria.Application.UseCase;

public interface IPasswordHash
{
    string Execute(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public class HashPassword : IPasswordHash
{

    public string Execute(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

