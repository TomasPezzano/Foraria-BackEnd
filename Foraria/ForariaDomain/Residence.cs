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

        public ICollection<User> Users { get; set; }

        public ICollection<Reserve> Reserves { get; set; }

        public  ICollection<UserDocument> UserDocuments { get; set; }
    }
}
