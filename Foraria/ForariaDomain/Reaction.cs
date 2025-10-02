namespace ForariaDomain
{
    public class Reaction
    {
        public int Id { get; set; }

        public int User_id { get; set; }
        public User User { get; set; }
        public int? Message_id { get; set; }
        public Message? Message { get; set; }

        public int? Thread_id { get; set; }
        public Thread? Thread { get; set; }

        public int ReactionType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
