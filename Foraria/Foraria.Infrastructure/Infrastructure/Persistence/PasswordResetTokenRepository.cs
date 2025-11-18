using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Infrastructure.Persistence;


public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ForariaContext _context;

    public PasswordResetTokenRepository(ForariaContext context)
    {
        _context = context;
    }

    public async Task Add(PasswordResetToken token)
    {
        await _context.PasswordResetTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<PasswordResetToken?> GetByToken(string token)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task Update(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PasswordResetToken>> GetActiveTokensByUserId(int userId)
    {
        return await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.Now)
            .ToListAsync();
    }
}