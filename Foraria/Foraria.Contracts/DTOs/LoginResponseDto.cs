namespace Foraria.Interface.DTOs;

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public bool RequiresPasswordChange { get; set; }
    public UserInfoDto User { get; set; }
}

public class UserInfoDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }

    public int? ResidenceId { get; set; }
    public int? ConsortiumId { get; set; }
}
