using Foraria.Domain.Service;

namespace ForariaDomain.Application.UseCase;

public class SendWelcomeEmailUseCase
{
    private readonly ISendEmail _emailService;

    public SendWelcomeEmailUseCase(ISendEmail emailService)
    {
        _emailService = emailService;
    }

    public async Task Execute(string toEmail, string firstName, string lastName, string temporaryPassword)
    {
        await _emailService.SendWelcomeEmail(toEmail, firstName, lastName, temporaryPassword);
    }
}

