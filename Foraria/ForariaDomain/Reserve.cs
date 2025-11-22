using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForariaDomain
{
    public class Reserve
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Description { get; set; }

        public string State { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int Place_id { get; set; }

        public Place Place { get; set; }

        public int User_id { get; set; }

        public User User { get; set; }
        public DateTime Date { get; set; }

        public int ConsortiumId { get; set; }
        public Consortium Consortium { get; set; }
    }
}
