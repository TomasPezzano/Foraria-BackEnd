using Foraria.Domain.Repository;
using ForariaDomain;


namespace Foraria.Application.UseCase;

public interface IUpdateUserFirstTime
{
    Task<UpdateUserFirstTimeResult> Update(User user, string currentPassword, string newPassword, string ipAddress);
}

public class UpdateUserFirstTimeResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}

public class UpdateUserFirstTime : IUpdateUserFirstTime
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHash _passwordHash;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRoleRepository _roleRepository;

    public UpdateUserFirstTime( IUserRepository userRepository, IPasswordHash passwordHash,  IJwtTokenGenerator jwtTokenGenerator, IRefreshTokenGenerator refreshTokenGenerator, IRefreshTokenRepository refreshTokenRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _passwordHash = passwordHash;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _roleRepository = roleRepository;
    }

    public async Task<UpdateUserFirstTimeResult> Update(User user, string currentPassword, string newPassword, string ipAddress)
    {

        if (user == null)
        {
            return new UpdateUserFirstTimeResult
            {
                Success = false,
                Message = "Usuario no encontrado"
            };
        }

        if (!user.RequiresPasswordChange)
        {
            return new UpdateUserFirstTimeResult
            {
                Success = false,
                Message = "Este usuario ya completó la actualización de datos"
            };
        }

        var isCurrentPasswordValid = _passwordHash.VerifyPassword(
            currentPassword,
            user.Password);

        if (!isCurrentPasswordValid)
        {
            return new UpdateUserFirstTimeResult
            {
                Success = false,
                Message = "La contraseña actual es incorrecta"
            };
        }

        if (!IsValidPassword(newPassword))
        {
            return new UpdateUserFirstTimeResult
            {
                Success = false,
                Message = "La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y caracteres especiales"
            };
        }


        user.Password = _passwordHash.HashPassword(newPassword);
        user.RequiresPasswordChange = false;

        await _userRepository.Update(user);

        Role? role = await _roleRepository.GetById(user.Role_id);
        if (role == null)
        {
            return new UpdateUserFirstTimeResult
            {
                Success = false,
                Message = "User role not found"
            };
        }
        user.Role = role;

        var accessToken = _jwtTokenGenerator.Generate(
            user.Id,
            user.Mail,
            user.Role_id,
            user.Role.Description,
            user.RequiresPasswordChange,
            false
        );

        var refreshToken = _refreshTokenGenerator.Generate();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress,
            IsRevoked = false
        };

        await _refreshTokenRepository.Add(refreshTokenEntity);

        return new UpdateUserFirstTimeResult
        {
            Success = true,
            Message = "Datos actualizados correctamente. Bienvenido a Foraria.",
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}