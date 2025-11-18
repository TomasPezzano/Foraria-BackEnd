using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace ForariaDomain
{
    public class Claim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Description { get; set; }

        public string State { get; set; }

        public string Priority { get; set; }

        public string Category { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public string Title { get; set; }

        public string? Archive { get; set; }

        public int? User_id { get; set; }

        public User? User { get; set; }

        public int? ClaimResponse_id { get; set; }

        public ClaimResponse? ClaimResponse { get; set; }

        [ForeignKey("Residence")]
        public int ResidenceId { get; set; }
        public Residence Residence { get; set; }

        public int ConsortiumId { get; set; }
        public Consortium Consortium { get; set; }
    }
}
