using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;

public class CallRepository : ICallRepository
{
    private readonly ForariaContext _context;

    public CallRepository(ForariaContext context)
    {
        _context = context;
    }

    public Call Create(Call call)
    {
        _context.Calls.Add(call);
        _context.SaveChanges();
        return call;
    }

    public Call? GetById(int id)
    {
        return _context.Calls.FirstOrDefault(c => c.Id == id);
    }

    public void Update(Call call)
    {
        _context.Calls.Update(call);
        _context.SaveChanges();
    }

    public List<Call> GetActiveCalls()
    {
        return _context.Calls.Where(c => c.Status == "Active").ToList();
    }

    public List<Call> GetByConsortium(int consortiumId)
    {
        return _context.Calls
            .Where(c => c.ConsortiumId == consortiumId)
            .OrderByDescending(c => c.StartedAt)
            .ToList();
    }

}
