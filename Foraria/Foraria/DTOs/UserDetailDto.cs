namespace Foraria.DTOs;

public class UserDetailDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mail { get; set; }
    public long PhoneNumber { get; set; }
    public string Role { get; set; }
    public List<ResidenceDto> Residences { get; set; }

    public int ConsortiumId {  get; set; }
}
