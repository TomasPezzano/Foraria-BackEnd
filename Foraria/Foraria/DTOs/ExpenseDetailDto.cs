using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.DTOs;

public class ExpenseDetailDto
{
    public int Id { get; set; }
    public double Total { get; set; }
    public string State { get; set; } = string.Empty;
    public int ExpenseId { get; set; }
    public ExpenseResponseDto? Expense { get; set; }
    public int ResidenceId { get; set; }
}
