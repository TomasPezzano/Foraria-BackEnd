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

public class SupplierRepository : ISupplierRepository
{
    private readonly ForariaContext _context;

    public SupplierRepository(ForariaContext context)
    {
        _context = context;
    }


    public Supplier Create(Supplier supplier)
    {
        _context.Suppliers.Add(supplier);
        _context.SaveChanges();
        return supplier;
    }

    public Supplier? GetById(int supplierId)
    {
        return _context.Suppliers
     .Include(s => s.Contracts)
     .FirstOrDefault(s => s.Id == supplierId);
    }

    public void Delete(int supplierId)
    {
        var supplier = _context.Suppliers.FirstOrDefault(s => s.Id == supplierId);
        if (supplier != null)
        {
            _context.Suppliers.Remove(supplier);
            _context.SaveChanges();
        }
    }

    public List<Supplier> GetAll()
    {
        return _context.Suppliers.ToList();
    }
}
