using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Models;
using Moq;

namespace ForariaTest.Unit.ClaimResponseTests
{
    public class CreateClaimResponseTests
    {
        private readonly Mock<IClaimResponseRepository> _claimResponseRepoMock;
        private readonly Mock<IClaimRepository> _claimRepoMock;
        private readonly CreateClaimResponse _useCase;

        public CreateClaimResponseTests()
        {
            _claimResponseRepoMock = new Mock<IClaimResponseRepository>();
            _claimRepoMock = new Mock<IClaimRepository>();
            _useCase = new CreateClaimResponse(_claimResponseRepoMock.Object, _claimRepoMock.Object);
        }

        [Fact]
        public async Task Execute_ShouldCreateResponse_AndSetClaimStateToEnProceso()
        {
            // Arrange
            var claim = new Claim { Id = 1, State = "Pendiente" };
            var user = new User { Id = 1, Name = "Juan" };

            var claimResponse = new ClaimResponse
            {
                Description = "Respuesta válida",
                ResponsibleSector_id = 2,
                Claim = claim,
                User = user
            };

            _claimResponseRepoMock
                .Setup(x => x.Add(It.IsAny<ClaimResponse>()))
                .Returns(Task.CompletedTask);

            _claimRepoMock
                .Setup(x => x.Update(It.IsAny<Claim>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _useCase.Execute(claimResponse);

            // Assert
            _claimResponseRepoMock.Verify(x => x.Add(It.IsAny<ClaimResponse>()), Times.Once);
            _claimRepoMock.Verify(x => x.Update(It.IsAny<Claim>()), Times.Once);

            Assert.Equal("En Proceso", claim.State);
            Assert.Same(claimResponse, claim.ClaimResponse);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Execute_ShouldThrowArgumentException_WhenClaimOrUserIsNull()
        {
            // Arrange
            var claimResponse1 = new ClaimResponse
            {
                Claim = null,
                User = new User(),
                ResponsibleSector_id = 1
            };

            var claimResponse2 = new ClaimResponse
            {
                Claim = new Claim(),
                User = null,
                ResponsibleSector_id = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(claimResponse1));
            await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(claimResponse2));
        }

        [Fact]
        public async Task Execute_ShouldThrowArgumentException_WhenSectorIsInvalid()
        {
            // Arrange
            var claimResponse = new ClaimResponse
            {
                Claim = new Claim(),
                User = new User(),
                ResponsibleSector_id = 0
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(claimResponse));
            Assert.Equal("Sector responsable inválido", ex.Message);
        }

        [Fact]
        public async Task Execute_ShouldPropagateException_WhenRepositoryFails()
        {
            // Arrange
            var claim = new Claim { Id = 1, State = "Pendiente" };
            var user = new User { Id = 1 };
            var claimResponse = new ClaimResponse
            {
                Claim = claim,
                User = user,
                ResponsibleSector_id = 2
            };

            _claimResponseRepoMock
                .Setup(x => x.Add(It.IsAny<ClaimResponse>()))
                .ThrowsAsync(new Exception("Error al guardar ClaimResponse"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _useCase.Execute(claimResponse));
            Assert.Equal("Error al guardar ClaimResponse", ex.Message);
        }
    }
}
