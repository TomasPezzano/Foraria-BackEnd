using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Contracts.DTOs;

public class SupplierContractResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContractType { get; set; }
    public string? Description { get; set; }
    public decimal MonthlyAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool Active { get; set; }
    public string? FilePath { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty; 
    public DateTime CreatedAt { get; set; }
}
