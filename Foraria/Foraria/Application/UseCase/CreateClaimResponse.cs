using System.Security.Claims;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public interface ICreateClaimResponse
{
    Task<ClaimResponseDto> Execute(ClaimResponseDto claimResponseDto);
}

public class CreateClaimResponse : ICreateClaimResponse
{
    private readonly IUserRepository _userRepository;
    private readonly IClaimResponseRepository _claimResponseRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IResponsibleSectorRepository _responsibleSectorRepository;
    private readonly ICreateClaimResponse _createClaimResponse;

    public CreateClaimResponse(IClaimResponseRepository claimResponseRepository, IClaimRepository claimRepository, IResponsibleSectorRepository responsibleSectorRepository, IUserRepository userRepository, ICreateClaimResponse createClaimResponse)
    {
        _claimResponseRepository = claimResponseRepository;
        _claimRepository = claimRepository;
        _responsibleSectorRepository = responsibleSectorRepository;
        _userRepository = userRepository;
        _createClaimResponse = createClaimResponse;
    }
    public async Task<ClaimResponseDto> Execute(ClaimResponseDto claimResponseDto)
    {

        var user = await _userRepository.GetById(claimResponseDto.User_id)
        ?? throw new ArgumentException("Usuario no existe");
        var claim = await _claimRepository.GetById(claimResponseDto.Claim_id)
            ?? throw new ArgumentException("Reclamo no existe");
        var sector = await _responsibleSectorRepository.GetById(claimResponseDto.ResponsibleSector_id)
            ?? throw new ArgumentException("ResponsibleSector no existe");


        var claimResponse = new ClaimResponse
        {
            Description = claimResponseDto.Description,
            ResponseDate = claimResponseDto.ResponseDate,
            User = user,
            ResponsibleSector_id = sector.Id

        };

        await _claimResponseRepository.Add(claimResponse);


        claim.State = "En Proceso";
        claim.ClaimResponse = claimResponse;
        await _claimRepository.Update(claim);


        var resultDto = new ClaimResponseDto
        {
            Description = claimResponse.Description,
            ResponseDate = claimResponse.ResponseDate,
            User_id = user.Id,
            Claim_id = claim.Id,
            ResponsibleSector_id = sector.Id
        };

        return resultDto;
    }
}
