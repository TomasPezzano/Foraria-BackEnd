using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase;

public interface IGetAllResidencesByConsortium
{
    Task<GetAllResidencesByConsortiumResult> ExecuteAsync(int consortiumId);
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
    private readonly IConsortiumRepository _consortiumRepository;

    public GetAllResidencesByConsortium(IResidenceRepository residenceRepository, IConsortiumRepository consortiumRepository)
    {
        _residenceRepository = residenceRepository;
        _consortiumRepository = consortiumRepository;
    }

    public async Task<GetAllResidencesByConsortiumResult> ExecuteAsync(int consortiumId)
    {
        if (consortiumId <= 0)
        {
            return new GetAllResidencesByConsortiumResult
            {
                Success = false,
                Message = "El ID del consorcio debe ser mayor a 0"
            };
        }

        var consortiumExists = await _consortiumRepository.FindById(consortiumId);
        if (consortiumExists == null)
        {
            return new GetAllResidencesByConsortiumResult
            {
                Success = false,
                Message = $"El consorcio con ID {consortiumId} no existe"
            };
        }

        var residences = await _residenceRepository.GetResidenceByConsortiumIdAsync(consortiumId);

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