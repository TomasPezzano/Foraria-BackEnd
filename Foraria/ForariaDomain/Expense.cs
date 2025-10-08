using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain
{
    public class Expense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Description { get; set; }

        public string State {  get; set; }

        public double TotalAmount { get; set; }

        public string Category { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpirationDate { get; set; }

        public int Id_Consortium { get; set; }

        public Consortium Consortium { get; set; }

        public int Id_Residence { get; set; }

        public Residence residence { get; set; }

        public ICollection<ExpenseDetail> ExpensesDetails { get; set; }

        public ICollection<Payment> payments { get; set; }
    }
}
