using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetAllResidencesByConsortium
{
    Task<GetAllResidencesByConsortiumResult> ExecuteAsync();
}

public class GetAllResidencesByConsortiumResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<Residence> Residences { get; set; } = new();
}

public class GetAllResidencesByConsortium : IGetAllResidencesByConsortium
{
    private readonly IResidenceRepository _residenceRepository;

    public GetAllResidencesByConsortium(IResidenceRepository residenceRepository)
    {
        _residenceRepository = residenceRepository;
    }

    public async Task<GetAllResidencesByConsortiumResult> ExecuteAsync()
    {
        var residences = await _residenceRepository.GetResidencesAsync();

        if (residences == null || !residences.Any())
        {
            return new GetAllResidencesByConsortiumResult
            {
                Success = true,
                Message = "El consorcio no tiene residencias asignadas",
                Residences = new List<Residence>()
            };
        }

        return new GetAllResidencesByConsortiumResult
        {
            Success = true,
            Message = "Residencias obtenidas exitosamente",
            Residences = residences
        };
    }
}