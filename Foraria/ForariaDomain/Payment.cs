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

        public string Voucher {  get; set; }

        public DateTime Date { get; set; }

        public int Id_Expense { get; set; }

        public Expense Expense { get; set; }

        public int Id_Residence { get; set; }

        public Residence Residence { get; set; }

        public int Id_PaymentMethod { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
    }
}
