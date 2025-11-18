using Foraria.Domain.Repository;
using ForariaDomain.Aplication.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtClaim = System.Security.Claims.Claim;


namespace ForariaDomain.Application.UseCase;

public interface IJwtTokenGenerator
{
    string Generate(int userId, string email, int roleId, string roleName, bool requiresPasswordChange, bool? hasPermission);
}


public class GenerateJwtToken : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserRepository _userRepository;

    public GenerateJwtToken(IOptions<JwtSettings> jwtSettings, IUserRepository userRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _userRepository = userRepository;
    }

    public string Generate(int userId, string email, int roleId, string roleName, bool requiresPasswordChange, bool? hasPermission)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var consortiumIds = _userRepository.GetConsortiumIdsByUserId(userId);

        var claims = new List<JwtClaim>
    {
        new JwtClaim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new JwtClaim(JwtRegisteredClaimNames.Email, email),
        new JwtClaim(ClaimTypes.Role, roleName),
        new JwtClaim("roleId", roleId.ToString()),
        new JwtClaim("hasPermission", hasPermission.ToString()),
        new JwtClaim("requiresPasswordChange", requiresPasswordChange.ToString()),
        new JwtClaim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        foreach (var cId in consortiumIds)
        {
            claims.Add(new JwtClaim("consortiumId", cId.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
