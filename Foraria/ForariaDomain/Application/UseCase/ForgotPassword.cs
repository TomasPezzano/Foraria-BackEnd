using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IForgotPassword
{
    Task<ForgotPasswordResult> Execute(string email, string ipAddress);
}

public class ForgotPasswordResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ForgotPassword : IForgotPassword
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordResetTokenGenerator _tokenGenerator;
    private readonly ISendEmail _emailService;

    public ForgotPassword(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordResetTokenGenerator tokenGenerator,
        ISendEmail emailService)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _tokenGenerator = tokenGenerator;
        _emailService = emailService;
    }

    public async Task<ForgotPasswordResult> Execute(string email, string ipAddress)
    {
        var user = await _userRepository.GetByEmail(email);

        if (user == null)
        {
            // Por seguridad, no revelamos si el email existe o no
            return new ForgotPasswordResult
            {
                Success = true,
                Message = "Si el correo existe en nuestro sistema, recibirás un enlace de restablecimiento"
            };
        }

        // Invalidar tokens activos previos
        var activeTokens = await _passwordResetTokenRepository.GetActiveTokensByUserId(user.Id);
        foreach (var token in activeTokens)
        {
            token.IsUsed = true;
            token.UsedAt = DateTime.Now;
            await _passwordResetTokenRepository.Update(token);
        }

        // Generar nuevo token
        var resetToken = _tokenGenerator.Generate();
        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = resetToken,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddMinutes(15),
            IsUsed = false,
            CreatedByIp = ipAddress
        };

        await _passwordResetTokenRepository.Add(passwordResetToken);

        // Enviar email
        await _emailService.SendPasswordResetEmail(user.Mail, user.Name, resetToken);

        return new ForgotPasswordResult
        {
            Success = true,
            Message = "Si el correo existe en nuestro sistema, recibirás un enlace de restablecimiento"
        };
    }
}
