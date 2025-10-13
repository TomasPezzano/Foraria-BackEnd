using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs
{
    public class NotarizeFileRequestDto
    {
        [Required]
        public IFormFile File { get; set; }

    }
}