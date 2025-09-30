namespace Foraria.Interface.DTOs
{
        public class CreateMessageRequest
        {
            public string Content { get; set; }
            public int Thread_id { get; set; }
            public int User_id { get; set; }
            public string? optionalFile { get; set; }
        }

        public class MessageResponse
        {
            public int Id { get; set; }
            public string Content { get; set; }
            public DateTime CreatedAt { get; set; }
            public string State { get; set; }
            public int Thread_id { get; set; }
            public int User_id { get; set; }
            public string? optionalFile { get; set; }
        }
    
}
