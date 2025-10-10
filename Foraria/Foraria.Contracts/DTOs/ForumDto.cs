using Foraria.Domain.Model;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs
{
    public class CreateForumRequest
    {
        [Required(ErrorMessage = "La categoría del foro es obligatoria.")]
        [EnumDataType(typeof(ForumCategory), ErrorMessage = "La categoría especificada no es válida.")]
        public ForumCategory Category { get; set; }
    }

    public class ForumResponse
    {
        public int Id { get; set; }
        public ForumCategory Category { get; set; }
    }
}