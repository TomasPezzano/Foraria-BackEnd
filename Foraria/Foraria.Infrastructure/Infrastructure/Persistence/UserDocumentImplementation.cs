using System.Security.Claims;
using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;

public class UserDocumentImplementation : IUserDocumentRepository
{

    private readonly ForariaContext _context;
    public UserDocumentImplementation(ForariaContext context)
    {
        _context = context;
    }
    public async Task Add(UserDocument UserDocument)
    {
        _context.UserDocuments.Add(UserDocument);
        await  _context.SaveChangesAsync();
    }

    public async Task<List<UserDocument>> GetAll()
    {
        return await _context.UserDocuments.ToListAsync();
    }

}
