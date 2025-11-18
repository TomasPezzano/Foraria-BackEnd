using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class ExpenseDetailByResidence
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public double TotalAmount { get; set; }
    public string State { get; set; } = string.Empty;
    public ICollection<Expense> Expenses { get; set; }

    [ForeignKey("Residence")]
    public int ResidenceId { get; set; }
    public Residence Residence { get; set; }

    public ICollection<Payment?> Payments { get; set; }
}
 