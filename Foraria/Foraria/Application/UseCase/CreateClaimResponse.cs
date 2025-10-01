using System.Security.Claims;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public class CreateClaimResponse
{

    private readonly IClaimResponseRepository _claimResponseRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IResponsibleSectorRepository _responsibleSectorRepository;

    public CreateClaimResponse(IClaimResponseRepository claimResponseRepository , IClaimRepository claimRepository, IResponsibleSectorRepository responsibleSectorRepository)
    {
        _claimResponseRepository = claimResponseRepository;
        _claimRepository = claimRepository;
        _responsibleSectorRepository = responsibleSectorRepository;
    }
    public void Execute(ClaimResponseDto claimResponseDto)
    {
       
        // var user = _userRepository.GetById(claimResponseDto.User_id);
        var claim = _claimRepository.GetById(claimResponseDto.Claim_id) ?? throw new ArgumentException("Reclamo no existe");
        var sector = _responsibleSectorRepository.GetById(claimResponseDto.ResponsibleSector_id) ?? throw new ArgumentException("ResponsibleSector no existe");
        
        var claimResponse = new ClaimResponse
        {
            Description = claimResponseDto.Description,
            ResponseDate = DateTime.Now,
            User = new User { Id = claimResponseDto.User_id },
            Claim = claim,
            ResponsibleSector_id = claimResponseDto.ResponsibleSector_id
        };
        
        _claimResponseRepository.Add(claimResponse);

        if (claim != null)
        {
            claim.State = "En Proceso";
            _claimRepository.Update(claim);
        }

    }
}
