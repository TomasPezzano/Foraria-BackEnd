using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class SelectConsortiumRequestDto
{
    [Required(ErrorMessage = "El ID del consorcio es requerido")]
    public int ConsortiumId { get; set; }
}

public class SelectConsortiumResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ConsortiumInfoDto ConsortiumInfo { get; set; }
}

public class ConsortiumInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
