using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs
{
    public class VerifyFileRequestDto
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        public Guid DocumentId { get; set; }
    }
}