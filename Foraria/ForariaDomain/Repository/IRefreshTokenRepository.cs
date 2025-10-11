using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> Add(RefreshToken refreshToken);
    Task<RefreshToken?> GetByToken(string token);
    Task<List<RefreshToken>> GetActiveByUserId(int userId);
    Task Update(RefreshToken refreshToken);
    Task RevokeAllByUserId(int userId);
    Task RemoveExpiredTokens();
}
