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

        public string Description { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public double TotalAmount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpirationDate { get; set; }

        [ForeignKey("Consortium")]
        public int ConsortiumId { get; set; }
        public Consortium Consortium { get; set; }

        [ForeignKey("Residence")]
        public int ResidenceId { get; set; }
        public Residence Residence { get; set; }

        public ICollection<ExpenseDetail> ExpenseDetails { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }
}
