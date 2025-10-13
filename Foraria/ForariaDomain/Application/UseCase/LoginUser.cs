using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain.Aplication.Configuration;
using Microsoft.Extensions.Options;

namespace Foraria.Application.UseCase;

public interface ILoginUser
{
    Task<LoginResponseDto> Login(LoginRequestDto loginDto, string ipAddress);
}

public class LoginUser : ILoginUser
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHash _passwordHash;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtSettings _jwtSettings;

    public LoginUser(
        IUserRepository userRepository,
        IPasswordHash passwordHash,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _passwordHash = passwordHash;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponseDto> Login(LoginRequestDto loginDto, string ipAddress)
    {
        var user = await _userRepository.GetByEmailWithRole(loginDto.Email);
        if (user == null)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        var isPasswordValid = _passwordHash.VerifyPassword(loginDto.Password, user.Password);
        if (!isPasswordValid)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        var accessToken = _jwtTokenGenerator.Generate(
            user.Id,
            user.Mail,
            user.Role_id,
            user.Role.Description,
            user.RequiresPasswordChange
        );

        var refreshToken = _refreshTokenGenerator.Generate();
        var refreshTokenEntity = new ForariaDomain.RefreshToken
        {
            UserId = user.Id,
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
            RequiresPasswordChange = user.RequiresPasswordChange,
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Mail,
                FirstName = user.Name,
                LastName = user.LastName,
                RoleId = user.Role_id,
                RoleName = user.Role.Description
            }
        };
    }
}