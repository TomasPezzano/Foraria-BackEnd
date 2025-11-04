using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace Foraria.Application.UseCase;

public interface ILogoutUser
{
    Task Logout(string refreshToken, string ipAddress);
}

public class LogoutUser : ILogoutUser
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutUser(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task Logout(string refreshToken, string ipAddress)
    {
        var storedToken = await _refreshTokenRepository.GetByToken(refreshToken);

        if (storedToken == null)
        {
            throw new NotFoundException("Invalid refresh token");
        }

        if (storedToken.IsActive)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress;
            await _refreshTokenRepository.Update(storedToken);
        }
    }
}