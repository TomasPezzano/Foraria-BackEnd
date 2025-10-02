using System.Security.Claims;
using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IClaimResponseRepository
{
    Task Add(ClaimResponse claimResponse);
}
