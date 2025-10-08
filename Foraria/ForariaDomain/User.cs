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

        public long? Dni { get; set; }
        public bool RequiresPasswordChange { get; set; }

        public long PhoneNumber { get; set; }

        public string? Photo { get; set; }

        public int Role_id { get; set; }

        public Role Role { get; set; }

        public ICollection<Claim> Claims { get; set; }

        public ICollection<ClaimResponse> ClaimsResponse { get; set; }

        public ICollection<Residence> Residence { get; set; }

        public ICollection<Reserve> Reserves { get; set; }

        //preguntar si esto es correcto
        public ICollection<Event> Events { get; set; }

        public ICollection<Thread> Threads { get; set; }

        public ICollection<Message> Messages { get; set; }

        public ICollection<Poll> Polls { get; set; }

        public ICollection<Vote> Votes { get; set; }

        public ICollection<UserDocument> UserDocuments { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }

    }
}
