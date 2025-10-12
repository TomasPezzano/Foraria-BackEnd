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

    public Task<User?> GetById(int id)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }


 
    public async Task<User?> GetByEmailWithRole(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Mail == email);
    }
    public async Task<int> GetTotalUsersAsync(int? consortiumId = null)
    {
        var query = _context.Users.AsQueryable();

        if (consortiumId.HasValue)
        {
            query = query
                .Include(u => u.Residences)
                .Where(u => u.Residences.Any(r => r.ConsortiumId == consortiumId.Value));
        }

        return await query.CountAsync();
    }
    public async Task<User?> GetByIdWithRole(int id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task Update(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

}
