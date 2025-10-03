namespace Foraria.Interface.DTOs
{
        public class CreateThreadRequest
        {
            public string Theme { get; set; }
            public string Description { get; set; }
            public int Forum_id { get; set; }
            public int User_id { get; set; }
        }

        public class ThreadResponse
        {
            public int Id { get; set; }
            public string Theme { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public string State { get; set; }
            public int Forum_id { get; set; }
            public int User_id { get; set; }
        }
    }

