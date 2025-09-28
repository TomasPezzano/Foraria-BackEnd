using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForariaDomain
{
    public class User
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }

        public string LastName { get; set; }

        public string Mail {  get; set; }

        public string Password { get; set; }

        public long Dni { get; set; }

        public long Telefono { get; set; }

        public string Foto { get; set; }

        [ForeignKey("Rol")] 
        public int Rol_id { get; set; }

        public Rol rol { get; set; }
    }
}
