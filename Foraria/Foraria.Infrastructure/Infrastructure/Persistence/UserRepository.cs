using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Diagnostics.Contracts;

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
            .Include(u => u.Residences)
                .ThenInclude(r => r.Consortium)
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
        return _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
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

    public async Task<int> GetAllInNumber()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> GetTotalUsersByTenantIdAsync(int idConsortium)
    {
        return await _context.Users
            .Where(u => u.Role.Description == "Inquilino" && u.Residences.Any(r => r.ConsortiumId == idConsortium))
            .CountAsync();
    }

    public async Task<int> GetTotalOwnerUsersAsync(int idConsortium)
    {
        return await _context.Users
            .Where(u => u.Role.Description == "Propietario" && u.Residences.Any(r => r.ConsortiumId == idConsortium))
            .CountAsync();
    }

    public async Task<List<User>> GetUsersByConsortiumIdAsync(int consortiumId)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Residences)
            .Where(u => u.Residences.Any(r => r.ConsortiumId == consortiumId))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsUserWithRoleInResidence(int residenceId, string roleDescription)
    {
        return await _context.Users
            .AnyAsync(u =>
                u.Role.Description.Equals(roleDescription) &&
                u.Residences.Any(r => r.Id == residenceId));
    }

}