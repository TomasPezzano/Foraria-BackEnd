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

    public SupplierContract Create(SupplierContract contract)
    {
        _context.SupplierContracts.Add(contract);
        _context.SaveChanges();
        return contract;
    }

    public SupplierContract? GetById(int id)
    {
        return _context.SupplierContracts
            .Include(c => c.Supplier)
            .FirstOrDefault(c => c.Id == id);
    }

    public List<SupplierContract> GetBySupplierId(int supplierId)
    {
        return _context.SupplierContracts
            .Include(c => c.Supplier)
            .Where(c => c.SupplierId == supplierId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    public List<SupplierContract> GetActiveContractsBySupplierId(int supplierId)
    {
        return _context.SupplierContracts
            .Include(c => c.Supplier)
            .Where(c => c.SupplierId == supplierId && c.Active)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    public SupplierContract Update(SupplierContract contract)
    {
        _context.SupplierContracts.Update(contract);
        _context.SaveChanges();
        return contract;
    }
}
