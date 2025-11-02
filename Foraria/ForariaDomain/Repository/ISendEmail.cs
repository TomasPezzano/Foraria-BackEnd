namespace Foraria.Domain.Service
{
    public interface ISendEmail
    {
        Task SendWelcomeEmail(string toEmail, string firstName, string lastName, string temporaryPassword);
        Task SendPasswordResetEmail(string toEmail, string firstName, string resetToken);

    }
}