using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IResetPassword
{
    Task<ResetPasswordResult> Execute(string token, string newPassword, string ipAddress);
}

public class ResetPasswordResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ResetPassword : IResetPassword
{
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHash _passwordHash;

    public ResetPassword(
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IUserRepository userRepository,
        IPasswordHash passwordHash)
    {
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _userRepository = userRepository;
        _passwordHash = passwordHash;
    }

    public async Task<ResetPasswordResult> Execute(string token, string newPassword, string ipAddress)
    {
        var resetToken = await _passwordResetTokenRepository.GetByToken(token);

        if (resetToken == null || !resetToken.IsValid)
        {
            return new ResetPasswordResult
            {
                Success = false,
                Message = "El enlace de restablecimiento es inválido o ha expirado"
            };
        }

        if (!IsValidPassword(newPassword))
        {
            return new ResetPasswordResult
            {
                Success = false,
                Message = "La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y caracteres especiales"
            };
        }

        var user = resetToken.User;
        user.Password = _passwordHash.Execute(newPassword);

        // Si el usuario tenía RequiresPasswordChange, lo mantenemos así
        // porque esto es un reset, no el primer login

        await _userRepository.Update(user);

        // Marcar token como usado
        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.Now;
        resetToken.UsedByIp = ipAddress;
        await _passwordResetTokenRepository.Update(resetToken);

        return new ResetPasswordResult
        {
            Success = true,
            Message = "Contraseña restablecida exitosamente. Ya puedes iniciar sesión"
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