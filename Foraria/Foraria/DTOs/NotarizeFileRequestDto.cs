using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs
{
    public class NotarizeFileRequestDto
    {
        [Required]
        public IFormFile File { get; set; }

    }
}