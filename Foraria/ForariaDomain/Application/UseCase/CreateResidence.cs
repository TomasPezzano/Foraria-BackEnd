using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface ICreateResidence
{
    Task<CreateResidenceResult> Create(int consortiumId, int number, int floor, string tower);
}

public class CreateResidenceResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Residence? Residence { get; set; }
}

public class CreateResidence : ICreateResidence
{
    private readonly IResidenceRepository _residenceRepository;
    private readonly IConsortiumRepository _consortiumRepository;

    public CreateResidence(IResidenceRepository residenceRepository, IConsortiumRepository consortiumRepository)
    {
        _residenceRepository = residenceRepository;
        _consortiumRepository = consortiumRepository;
    }

    public async Task<CreateResidenceResult> Create(int consortiumId, int number, int floor, string tower)
    {
        if (string.IsNullOrWhiteSpace(tower))
        {
            return new CreateResidenceResult
            {
                Success = false,
                Message = "La torre no puede estar vacía"
            };
        }

        if (number == 0)
        {
            return new CreateResidenceResult
            {
                Success = false,
                Message = "El número no puede estar vacío"
            };
        }

        var consortiumExists = await _consortiumRepository.FindById(consortiumId);
        if (consortiumExists == null)
        {
            return new CreateResidenceResult
            {
                Success = false,
                Message = $"El consorcio con ID {consortiumId} no existe"
            };
        }

        var existingResidences = await _residenceRepository.GetResidenceByConsortiumIdAsync(consortiumId);
        if (existingResidences.Any(r =>
            r.Number == number &&
            r.Floor == floor &&
            r.Tower.Equals(tower, StringComparison.OrdinalIgnoreCase)))
        {
            return new CreateResidenceResult
            {
                Success = false,
                Message = "Ya existe una vivienda con ese número, piso y torre"
            };
        }

        var residence = new Residence
        {
            Number = number,
            Floor = floor,
            Tower = tower,
            ConsortiumId = consortiumId
        };

        var createdResidence = await _residenceRepository.Create(residence, consortiumId);

        return new CreateResidenceResult
        {
            Success = true,
            Message = "Vivienda creada exitosamente",
            Residence = createdResidence
        };
    }
}