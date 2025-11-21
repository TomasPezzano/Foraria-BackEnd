using FluentAssertions;
using Foraria.Application.UseCase;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;
using System;
using Xunit;

namespace ForariaTest.Unit.Calls
{
    public class CreateCallTests
    {
        [Fact]
        public void Execute_ShouldCreateCall_WithProvidedData()
        {
            // Arrange
            var inputCall = new Call
            {
                CreatedByUserId = 10,
                Title = "Reunión mensual",
                Description = "Revisión de expensas",
                MeetingType = "Consorcio",
                ConsortiumId = 5
            };

            var returnedCall = new Call
            {
                Id = 1,
                CreatedByUserId = inputCall.CreatedByUserId,
                Title = inputCall.Title,
                Description = inputCall.Description,
                MeetingType = inputCall.MeetingType,
                ConsortiumId = inputCall.ConsortiumId,
                StartedAt = DateTime.Now,
                Status = "Active"
            };

            var mockRepo = new Mock<ICallRepository>();

            mockRepo
                .Setup(r => r.Create(It.IsAny<Call>()))
                .Returns(returnedCall);

            var useCase = new CreateCall(mockRepo.Object);

            // Act
            var result = useCase.Execute(inputCall);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.CreatedByUserId.Should().Be(inputCall.CreatedByUserId);

            result.Title.Should().Be(inputCall.Title);
            result.Description.Should().Be(inputCall.Description);
            result.MeetingType.Should().Be(inputCall.MeetingType);
            result.ConsortiumId.Should().Be(inputCall.ConsortiumId);

            result.Status.Should().Be("Active");
            result.StartedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));

            mockRepo.Verify(r => r.Create(It.IsAny<Call>()), Times.Once);
        }
    }
}
