using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain
{
    public class ExpenseDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public double Amount { get; set; }

        public string Description { get; set; }

        public int ExpenseId { get; set; }

        public Expense Expense { get; set; }
    }
}
