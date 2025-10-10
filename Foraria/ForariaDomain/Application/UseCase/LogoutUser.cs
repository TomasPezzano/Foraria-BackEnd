using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase;

public interface ILogoutUser
{
    Task<LogoutResponseDto> Logout(string refreshToken, string ipAddress);
}

public class LogoutUser : ILogoutUser
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutUser(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<LogoutResponseDto> Logout(string refreshToken, string ipAddress)
    {
        // 1. Get refresh token from database
        var storedToken = await _refreshTokenRepository.GetByToken(refreshToken);

        if (storedToken == null)
        {
            return new LogoutResponseDto
            {
                Success = false,
                Message = "Invalid refresh token"
            };
        }

        // 2. Revoke the refresh token
        if (storedToken.IsActive)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress;
            await _refreshTokenRepository.Update(storedToken);
        }

        return new LogoutResponseDto
        {
            Success = true,
            Message = "Logout successful"
        };
    }
}