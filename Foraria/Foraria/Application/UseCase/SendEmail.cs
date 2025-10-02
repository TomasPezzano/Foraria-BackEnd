using Foraria.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Foraria.Application.UseCase;

public interface ISendEmail
{
    Task SendWelcomeEmail(string toEmail, string firstName, string lastName, string temporaryPassword);
}

public class SendEmail : ISendEmail
{
    private readonly EmailSettings _emailSettings;

    public SendEmail(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendWelcomeEmail(string toEmail, string firstName, string lastName, string temporaryPassword)
    {
        var subject = "Bienvenido a Foraria - Tus Credenciales de Acceso";
        var body = BuildWelcomeEmailBody(firstName, lastName, toEmail, temporaryPassword);

        await SendEmailAsync(toEmail, subject, body);
    }

    private string BuildWelcomeEmailBody(string email,string firstName, string lastName, string password)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .content {{
            background-color: white;
            padding: 30px;
            border-radius: 5px;
        }}
        .header {{
            background-color: #4CAF50;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .credentials {{
            background-color: #f9f9f9;
            padding: 15px;
            margin: 20px 0;
            border-left: 4px solid #4CAF50;
        }}
        .credential-item {{
            margin: 10px 0;
        }}
        .credential-label {{
            font-weight: bold;
            color: #555;
        }}
        .credential-value {{
            color: #333;
            font-family: 'Courier New', monospace;
            background-color: #fff;
            padding: 5px 10px;
            border-radius: 3px;
            display: inline-block;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #4CAF50;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            color: #777;
            font-size: 12px;
        }}
        .warning {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='content'>
            <div class='header'>
                <h1>¡Bienvenido a Foraria!</h1>
            </div>
            
            <p>Hola <strong>{firstName + lastName}</strong>,</p>
            
            <p>Tu cuenta ha sido creada exitosamente. A continuación encontrarás tus credenciales de acceso:</p>
            
            <div class='credentials'>
                <div class='credential-item'>
                    <span class='credential-label'>Usuario (Email):</span><br/>
                    <span class='credential-value'>{email}</span>
                </div>
                <div class='credential-item'>
                    <span class='credential-label'>Contraseña Temporal:</span><br/>
                    <span class='credential-value'>{password}</span>
                </div>
            </div>
            
            <div class='warning'>
                <strong>⚠️ Importante:</strong> Esta es una contraseña temporal. Se te solicitará cambiarla en tu primer inicio de sesión.
            </div>
            
            <p style='text-align: center;'>
                <a href='http://localhost:3000/login' class='button'>Iniciar Sesión</a>
            </p>
            
            <p>Si tienes alguna pregunta o problema para acceder, no dudes en contactar con soporte.</p>
            
            <p>Saludos,<br/>
            <strong>El equipo de Foraria</strong></p>
            
            <div class='footer'>
                <p>Este es un correo automático, por favor no responder.</p>
                <p>&copy; 2025 Foraria. Todos los derechos reservados.</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _emailSettings.Username,
                _emailSettings.Password)
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
