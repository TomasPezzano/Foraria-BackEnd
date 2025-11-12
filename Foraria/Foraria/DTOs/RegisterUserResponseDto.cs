namespace Foraria.DTOs;

public class RegisterUserResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int? Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public long PhoneNumber { get; set; }
    public int RoleId { get; set; }
    public string TemporaryPassword { get; set; }

    public List<ResidenceResponseDto>? Residences { get; set; }

}
