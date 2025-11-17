namespace Foraria.DTOs
{
    public class UpdateMessageRequest
    {
        public int UserId { get; set; }
        public string? Content { get; set; }
        public IFormFile? File { get; set; }
        public string? FilePathToUpdate { get; set; }
        public bool RemoveFile { get; set; } = false;
    }
}