using Foraria.Domain.Repository;
using ForariaDomain.Aplication.Configuration;
using ForariaDomain.Exceptions;
using ForariaDomain.Models;
using Microsoft.Extensions.Options;


namespace ForariaDomain.Application.UseCase;

public interface ILoginUser
{
    Task<LoginResult> Login(User usuarioLogueado, string passRequest, string ipAddress);
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

    public async Task<LoginResult> Login(User usuarioLogueado, string passRequest, string ipAddress)
    {
        if (usuarioLogueado == null)
            throw new BusinessException("Invalid email or password");

        var isPasswordValid = _passwordHash.VerifyPassword(passRequest, usuarioLogueado.Password);
        if (!isPasswordValid)
            throw new BusinessException("Invalid email or password");

        Role? role = await _roleRepository.GetById(usuarioLogueado.Role_id);
        if (role == null)
            throw new NotFoundException("User role not found");


        usuarioLogueado.Role = role;

        var accessToken = _jwtTokenGenerator.Generate(
            usuarioLogueado.Id,
            usuarioLogueado.Mail,
            usuarioLogueado.Role_id,
            usuarioLogueado.Role.Description,
            usuarioLogueado.RequiresPasswordChange,
            usuarioLogueado.HasPermission
        );

        var refreshToken = _refreshTokenGenerator.Generate();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = usuarioLogueado.Id,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress,
            IsRevoked = false
        };

        await _refreshTokenRepository.Add(refreshTokenEntity);

        return LoginResult.SuccessResult(accessToken, refreshToken, usuarioLogueado);

    }
}