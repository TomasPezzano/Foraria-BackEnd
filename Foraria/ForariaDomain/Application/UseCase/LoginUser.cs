using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Aplication.Configuration;
using Microsoft.Extensions.Options;

namespace Foraria.Application.UseCase;

public interface ILoginUser
{
    Task<LoginResponseDto> Login(User usuarioLogueado, string passRequest, string ipAddress);
}

public class LoginUser : ILoginUser
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHash _passwordHash;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly IRoleRepository _roleRepository;

    public LoginUser(
        IUserRepository userRepository,
        IPasswordHash passwordHash,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtSettings> jwtSettings,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _passwordHash = passwordHash;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtSettings = jwtSettings.Value;
        _roleRepository = roleRepository;
    }

    public async Task<LoginResponseDto> Login(User usuarioLogueado, string passRequest, string ipAddress)
    {
        if (usuarioLogueado == null)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        var isPasswordValid = _passwordHash.VerifyPassword(passRequest, usuarioLogueado.Password);
        if (!isPasswordValid)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        Role? role = await _roleRepository.GetById(usuarioLogueado.Role_id);
        if (role == null)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "User role not found"
            };
        }
        usuarioLogueado.Role = role;

        var accessToken = _jwtTokenGenerator.Generate(
            usuarioLogueado.Id,
            usuarioLogueado.Mail,
            usuarioLogueado.Role_id,
            usuarioLogueado.Role.Description,
            usuarioLogueado.RequiresPasswordChange
        );

        var refreshToken = _refreshTokenGenerator.Generate();
        var refreshTokenEntity = new ForariaDomain.RefreshToken
        {
            UserId = usuarioLogueado.Id,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress,
            IsRevoked = false
        };

        await _refreshTokenRepository.Add(refreshTokenEntity);

        return new LoginResponseDto
        {
            Success = true,
            Message = "Login successful",
            Token = accessToken,
            RefreshToken = refreshToken,
            RequiresPasswordChange = usuarioLogueado.RequiresPasswordChange,
            User = new UserInfoDto
            {
                Id = usuarioLogueado.Id,
                Email = usuarioLogueado.Mail,
                FirstName = usuarioLogueado.Name,
                LastName = usuarioLogueado.LastName,
                RoleId = usuarioLogueado.Role_id,
                RoleName = usuarioLogueado.Role.Description
            }
        };
    }
}