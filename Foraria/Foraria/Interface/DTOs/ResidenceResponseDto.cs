namespace Foraria.Interface.DTOs;

public class ResidenceResponseDto
{
    public int? Id { get; set; }
    public int Number { get; set; }
    public int Floor { get; set; }
    public string Tower { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}
