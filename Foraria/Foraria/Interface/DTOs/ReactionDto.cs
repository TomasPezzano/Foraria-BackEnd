namespace Foraria.Interface.DTOs
{
    public class ReactionRequest
    {
        public int User_id { get; set; }
        public int? Message_id { get; set; }
        public int? Thread_id { get; set; }
        public int ReactionType { get; set; } // +1 like -1 dislike
    }
}