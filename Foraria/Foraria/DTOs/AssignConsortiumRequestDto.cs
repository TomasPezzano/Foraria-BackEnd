using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class AssignConsortiumRequestDto
{
    [Required(ErrorMessage = "El ID del administrador es requerido")]
    public int AdministratorId { get; set; }

    [Required(ErrorMessage = "El ID del consorcio es requerido")]
    public int ConsortiumId { get; set; }
}

public class AssignConsortiumResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public AdminConsortiumsDto AdminInfo { get; set; }
}

public class AdminConsortiumsDto
{
    public int AdministratorId { get; set; }
    public string AdministratorName { get; set; }
    public string AdministratorEmail { get; set; }
    public List<ConsortiumInfoDto> AssignedConsortiums { get; set; }
}
