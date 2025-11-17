using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain
{
    public class Residence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Number { get; set; }

        public int Floor { get; set; }

        public string Tower { get; set; }

        public double Coeficient { get; set; }

        public ICollection<User> Users { get; set; }

        public ICollection<Reserve> Reserves { get; set; }

        public ICollection<ExpenseDetailByResidence> ExpenseDetailByResidence { get; set; }

        public ICollection<Payment> Payments { get; set; }

        [ForeignKey("Consortium")]
        public int ConsortiumId { get; set; }
        public Consortium Consortium { get; set; }

        public ICollection<Claim> Claims { get; set; }

        public ICollection<Expense> Expenses { get; set; }

        public ICollection<Invoice> Invoices{ get; set; }

    }
}
