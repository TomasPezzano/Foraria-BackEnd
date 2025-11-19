using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class Consortium
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int? AdministratorId { get; set; }

    public User? Administrator { get; set; }

    public ICollection<UserDocument> UserDocuments { get; set; }

    public ICollection<Expense> Expenses { get; set; }

    public ICollection<Supplier> Suppliers { get; set; }
    public ICollection<Residence> Residences { get; set; }

    public ICollection<Invoice> Invoices { get; set; }

    public ICollection<Reserve> Reserves { get; set; }

    public ICollection<Claim?> Claims { get; set; }

    public ICollection<Poll?> Polls { get; set; }

    public ICollection<Call?> Calls { get; set; }

    public ICollection<Thread?> Threads { get; set; }

    public ICollection<Forum?> Forums { get; set; }
}
