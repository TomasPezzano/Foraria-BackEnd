using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit
{
    public class GetResponsibleSectorByIdTests
    {
        private readonly Mock<IResponsibleSectorRepository> _mockRepo;
        private readonly GetResponsibleSectorById _useCase;

        public GetResponsibleSectorByIdTests()
        {
            _mockRepo = new Mock<IResponsibleSectorRepository>();
            _useCase = new GetResponsibleSectorById(_mockRepo.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnResponsibleSector_WhenSectorExists()
        {
            int sectorId = 1;
            var sector = new ResponsibleSector
            {
                Id = sectorId,
                Name = "Mantenimiento"
            };

            _mockRepo.Setup(r => r.GetById(sectorId))
                     .ReturnsAsync(sector);

            var result = await _useCase.Execute(sectorId);

            Assert.NotNull(result);
            Assert.Equal(sectorId, result.Id);
            Assert.Equal("Mantenimiento", result.Name);

            _mockRepo.Verify(r => r.GetById(sectorId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrowInvalidOperationException_WhenSectorDoesNotExist()
        {
            int sectorId = 999;

            _mockRepo.Setup(r => r.GetById(sectorId))
                     .ReturnsAsync((ResponsibleSector?)null);

            Func<Task> act = async () => await _useCase.Execute(sectorId);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.Equal($"ResponsibleSector with id {sectorId} not found.", exception.Message);

            _mockRepo.Verify(r => r.GetById(sectorId), Times.Once);
        }
    }
}
