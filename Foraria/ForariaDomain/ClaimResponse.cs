using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain
{
    public class ClaimResponse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Description { get; set; }

        public DateTime ResponseDate { get; set; }

        public Claim Claim { get; set; }

        public User User { get; set; }

        public int ResponsibleSector_id { get; set; }

        public ResponsibleSector ResponsibleSector { get; set; }



    }
}
