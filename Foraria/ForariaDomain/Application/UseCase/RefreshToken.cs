using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase;

public interface IRefreshTokenUseCase
{
    Task<RefreshTokenResponseDto> Refresh(string refreshToken, string ipAddress);
}
public class RefreshToken : IRefreshTokenUseCase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public RefreshToken(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<RefreshTokenResponseDto> Refresh(string refreshToken, string ipAddress)
    {
        // 1. Get refresh token from database
        var storedToken = await _refreshTokenRepository.GetByToken(refreshToken);

        // 2. Validate refresh token exists
        if (storedToken == null)
        {
            return new RefreshTokenResponseDto
            {
                Success = false,
                Message = "Invalid refresh token"
            };
        }

        // 3. Validate refresh token is active
        if (!storedToken.IsActive)
        {
            return new RefreshTokenResponseDto
            {
                Success = false,
                Message = storedToken.IsRevoked ? "Token has been revoked" : "Token has expired"
            };
        }

        // 4. Generate new access token
        var newAccessToken = _jwtTokenGenerator.Generate(
            storedToken.User.Id,
            storedToken.User.Mail,
            storedToken.User.Role_id,
            storedToken.User.Role.Description,
            storedToken.User.RequiresPasswordChange
        );

        // 5. (OPCIONAL) Refresh Token Rotation - Generar nuevo refresh token
        var newRefreshToken = _refreshTokenGenerator.Generate();
        var newRefreshTokenEntity = new ForariaDomain.RefreshToken
        {
            UserId = storedToken.UserId,
            Token = newRefreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress,
            IsRevoked = false
        };

        // 6. Revocar el refresh token viejo (rotation)
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = ipAddress;
        storedToken.ReplacedByToken = newRefreshToken;

        await _refreshTokenRepository.Update(storedToken);
        await _refreshTokenRepository.Add(newRefreshTokenEntity);

        // 7. Return new tokens
        return new RefreshTokenResponseDto
        {
            Success = true,
            Message = "Token refreshed successfully",
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}
