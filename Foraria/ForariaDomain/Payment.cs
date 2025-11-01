using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Voucher { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        [ForeignKey(nameof(ExpenseDetailByResidence))]
        public int ExpenseDetailByResidenceId { get; set; }
        public ExpenseDetailByResidence ExpenseDetailByResidence { get; set; }

        [ForeignKey(nameof(Residence))]
        public int ResidenceId { get; set; }
        public Residence Residence { get; set; }

        [ForeignKey(nameof(PaymentMethod))]
        public int PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
