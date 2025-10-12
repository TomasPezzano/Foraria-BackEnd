using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;

namespace Foraria.Application.UseCase;

public interface IRefreshTokenUseCase
{
    Task<RefreshTokenResponseDto> Refresh(string refreshToken, string ipAddress);
}
public class RefreshTokenUseCase : IRefreshTokenUseCase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public RefreshTokenUseCase(
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
        var storedToken = await _refreshTokenRepository.GetByToken(refreshToken);

        if (storedToken == null)
        {
            return new RefreshTokenResponseDto
            {
                Success = false,
                Message = "Invalid refresh token"
            };
        }

        if (!storedToken.IsActive)
        {
            return new RefreshTokenResponseDto
            {
                Success = false,
                Message = storedToken.IsRevoked ? "Token has been revoked" : "Token has expired"
            };
        }

        var newAccessToken = _jwtTokenGenerator.Generate(
            storedToken.User.Id,
            storedToken.User.Mail,
            storedToken.User.Role_id,
            storedToken.User.Role.Description,
            storedToken.User.RequiresPasswordChange
        );

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

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = ipAddress;
        storedToken.ReplacedByToken = newRefreshToken;

        await _refreshTokenRepository.Update(storedToken);
        await _refreshTokenRepository.Add(newRefreshTokenEntity);

        return new RefreshTokenResponseDto
        {
            Success = true,
            Message = "Token refreshed successfully",
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}
