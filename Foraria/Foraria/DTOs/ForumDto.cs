using Foraria.Domain.Model;
using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs
{
    public class ForumDto
    {
        public int Id { get; set; }
        public ForumCategory Category { get; set; }
        public List<ThreadDto>? Threads { get; set; }
    }
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
        public string CategoryName { get; set; }
        public int CountThreads { get; set; }
        public int CountResponses { get; set; }
        public int CountUserActives { get; set; }
    }

    public class ForumWithCategoryResponse
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int CategoryValue { get; set; }
        public int ThreadCount { get; set; }
    }
}