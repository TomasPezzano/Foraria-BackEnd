using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;
public interface ICreateClaim
{
    Task<Claim> Execute(ClaimDto claimDto);
}
public class CreateClaim : ICreateClaim
{
    private readonly IClaimRepository _claimRepository;
    
    public CreateClaim(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }
    public async Task<Claim> Execute(ClaimDto claimDto)
    {
        if (string.IsNullOrWhiteSpace(claimDto.Title))
            throw new ArgumentException("El título del reclamo es obligatorio");
        if (string.IsNullOrWhiteSpace(claimDto.Description))
            throw new ArgumentException("La descripción del reclamo es obligatoria");
        if (string.IsNullOrWhiteSpace(claimDto.Priority))
            throw new ArgumentException("La prioridad es obligatoria");
        if (string.IsNullOrWhiteSpace(claimDto.Category))
            throw new ArgumentException("La categoría es obligatoria");
        if (claimDto.User_id == null)
            throw new ArgumentException("Debe asociarse un usuario al reclamo");

        var claim = new Claim
        {
            Title = claimDto.Title,
            Description = claimDto.Description,
            State = "Nuevo",
            Priority = claimDto.Priority,
            Category = claimDto.Category,
            CreatedAt = DateTime.Now,
            Archive = claimDto.Archive,
            User_id = claimDto.User_id,
            ResidenceId = claimDto.ResidenceId
        };

        await _claimRepository.Add(claim);
        return claim;
    }

}
