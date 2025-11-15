using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Infrastructure.Infrastructure.Persistence;

public class SupplierContractRepository : ISupplierContractRepository
{
    private readonly ForariaContext _context;

    public SupplierContractRepository(ForariaContext context)
    {
        _context = context;
    }

    public async Task<SupplierContract> Create(SupplierContract contract)
    {
        await _context.SupplierContracts.AddAsync(contract);
        await _context.SaveChangesAsync();
        return contract;
    }

    public async Task<SupplierContract?> GetById(int id)
    {
        return await _context.SupplierContracts
            .Include(c => c.Supplier)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<SupplierContract>> GetBySupplierId(int supplierId)
    {
        return _context.SupplierContracts
            .Include(c => c.Supplier)
            .Where(c => c.SupplierId == supplierId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    public async Task<List<SupplierContract>> GetActiveContractsBySupplierId(int supplierId)
    {
        return await _context.SupplierContracts
            .Include(c => c.Supplier)
            .Where(c => c.SupplierId == supplierId && c.Active)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupplierContract> Update(SupplierContract contract)
    {
        _context.SupplierContracts.Update(contract);
        await _context.SaveChangesAsync();
        return contract;
    }

    public async Task<int> GetActiveContractsCount(int consortiumId)
    {
        return await _context.SupplierContracts
            .Where(c => c.Active &&
                        c.EndDate >= DateTime.UtcNow &&     
                        c.Supplier.ConsortiumId == consortiumId)
            .CountAsync();
    }
}
