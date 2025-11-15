using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Domain.Service;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit
{
    public class SendWelcomeEmailUseCaseTests
    {
        [Fact]
        public async Task Execute_ShouldCallSendWelcomeEmail_WithCorrectParameters()
        {
            var emailServiceMock = new Mock<ISendEmail>();
            var useCase = new SendWelcomeEmailUseCase(emailServiceMock.Object);

            string toEmail = "test@example.com";
            string firstName = "John";
            string lastName = "Doe";
            string temporaryPassword = "Temp1234!";

            emailServiceMock
                .Setup(e => e.SendWelcomeEmail(toEmail, firstName, lastName, temporaryPassword))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await useCase.Execute(toEmail, firstName, lastName, temporaryPassword);

            emailServiceMock.Verify(
                e => e.SendWelcomeEmail(toEmail, firstName, lastName, temporaryPassword),
                Times.Once
            );
        }
    }
}
