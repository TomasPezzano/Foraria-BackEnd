using Foraria.Interface.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Contracts.DTOs;

public class UserDetailDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mail { get; set; }
    public long PhoneNumber { get; set; }
    public string Role { get; set; }
    public List<ResidenceDto> Residences { get; set; }
}
