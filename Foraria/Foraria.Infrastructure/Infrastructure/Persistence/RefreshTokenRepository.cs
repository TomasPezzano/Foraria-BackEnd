using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ForariaContext _context;

    public RefreshTokenRepository(ForariaContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken> Add(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<RefreshToken?> GetByToken(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<List<RefreshToken>> GetActiveByUserId(int userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsRevoked == false && rt.ExpiresAt > DateTime.Now)
            .ToListAsync();
    }

    public async Task Update(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllByUserId(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsRevoked == false)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
    }

    public async Task RemoveExpiredTokens()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.Now)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
}
