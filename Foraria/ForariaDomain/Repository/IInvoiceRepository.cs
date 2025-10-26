using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository;

public interface IInvoiceRepository
{
    Task<Invoice> CreateInvoice(Invoice invoice);
    Task<IEnumerable<Invoice>> GetAllInvoices();
}
