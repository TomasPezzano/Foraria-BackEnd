using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace Foraria.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly ForariaContext _context;
    public UserRepository(ForariaContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmail(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Mail == email);
    }

    public async Task<User> Add(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ExistsEmail(string email)
    {
        return await _context.Users.AnyAsync(u => u.Mail == email);
    }

}
