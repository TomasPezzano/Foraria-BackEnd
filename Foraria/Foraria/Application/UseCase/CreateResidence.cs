using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public interface ICreateResidence
{
    Task<ResidenceResponseDto> Create(ResidenceRequestDto residence);
    Task<ResidenceResponseDto> GetResidenceById(int id);
    Task<List<ResidenceResponseDto>> GetAllResidences();
}

public class CreateResidence : ICreateResidence
{
    private readonly IResidenceRepository _residenceRepository;

    public CreateResidence(IResidenceRepository residenceRepository)
    {
        _residenceRepository = residenceRepository;
    }

    public async Task<ResidenceResponseDto> Create(ResidenceRequestDto residenceDto)
    {
        if (string.IsNullOrWhiteSpace(residenceDto.Tower))
        {
            return new ResidenceResponseDto
            {
                Success = false,
                Message = "La torre no puede estar vacía"
            };
        }

        var existingResidences = await _residenceRepository.GetAll();
        if (existingResidences.Any(r =>
            r.Number == residenceDto.Number &&
            r.Floor == residenceDto.Floor &&
            r.Tower.Equals(residenceDto.Tower, StringComparison.OrdinalIgnoreCase)))
        {
            return new ResidenceResponseDto
            {
                Success = false,
                Message = "Ya existe una vivienda con ese número, piso y torre"
            };
        }

        var residence = new Residence
        {
            Number = residenceDto.Number,
            Floor = residenceDto.Floor,
            Tower = residenceDto.Tower
        };

        var createdResidence = await _residenceRepository.Create(residence);

        return new ResidenceResponseDto
        {
            Number = createdResidence.Number,
            Floor = createdResidence.Floor,
            Tower = createdResidence.Tower,
            Success = true,
            Message = "Vivienda creada exitosamente"
        };
    }

    public async Task<ResidenceResponseDto> GetResidenceById(int id)
    {
        var residence = await _residenceRepository.GetById(id);

        if (residence == null)
        {
            return new ResidenceResponseDto
            {
                Success = false,
                Message = "Vivienda no encontrada"
            };
        }

        return new ResidenceResponseDto
        {
            Number = residence.Number,
            Floor = residence.Floor,
            Tower = residence.Tower,
            Success = true,
            Message = "Vivienda obtenida exitosamente"
        };
    }

    public async Task<List<ResidenceResponseDto>> GetAllResidences()
    {
        var residences = await _residenceRepository.GetAll();
        return residences.Select(r => new ResidenceResponseDto
        {
            Number = r.Number,
            Floor = r.Floor,
            Tower = r.Tower,
            Success = true
        }).ToList();
    }
}