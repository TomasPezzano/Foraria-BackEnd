using ForariaDomain;

namespace Foraria.DTOs;

public class ForumWithCategoryDto
{
    public int Id { get; set; }
    public ForumCategory Category { get; set; }
    public string CategoryName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

