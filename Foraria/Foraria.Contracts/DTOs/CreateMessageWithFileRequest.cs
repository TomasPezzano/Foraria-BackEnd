using Microsoft.AspNetCore.Http;

namespace Foraria.Interface.DTOs
{
    public class CreateMessageWithFileRequest
    {
        public string Content { get; set; }
        public int Thread_id { get; set; }
        public int User_id { get; set; }
        public IFormFile? File { get; set; }
        public string? FilePath { get; set; }
    }
}