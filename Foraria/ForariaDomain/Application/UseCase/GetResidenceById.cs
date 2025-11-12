using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetResidenceById
{
    Task<GetResidenceByIdResult> Execute(int residenceId);
}

public class GetResidenceByIdResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Residence? Residence { get; set; }
}

public class GetResidenceById : IGetResidenceById
{
    private readonly IResidenceRepository _residenceRepository;

    public GetResidenceById(IResidenceRepository residenceRepository)
    {
        _residenceRepository = residenceRepository;
    }

    public async Task<GetResidenceByIdResult> Execute(int residenceId)
    {
        if (residenceId <= 0)
        {
            return new GetResidenceByIdResult
            {
                Success = false,
                Message = "El ID de la residencia debe ser mayor a 0"
            };
        }

        var residence = await _residenceRepository.GetById(residenceId);

        if (residence == null)
        {
            return new GetResidenceByIdResult
            {
                Success = false,
                Message = $"No se encontró la residencia con ID {residenceId}"
            };
        }

        return new GetResidenceByIdResult
        {
            Success = true,
            Message = "Residencia encontrada",
            Residence = residence
        };
    }
}