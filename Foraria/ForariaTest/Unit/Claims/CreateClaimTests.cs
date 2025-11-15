using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Claims
{
    public class CreateClaimTests
    {
        private readonly Mock<IClaimRepository> _claimRepositoryMock;
        private readonly CreateClaim _useCase;

        public CreateClaimTests()
        {
            _claimRepositoryMock = new Mock<IClaimRepository>();
            _useCase = new CreateClaim(_claimRepositoryMock.Object);
        }

        [Fact]
        public async Task Execute_ShouldAddClaim_WhenDataIsValid()
        {
            var claim = new Claim
            {
                Title = "Título válido",
                Description = "Descripción válida",
                Priority = "Alta",
                Category = "General",
                User_id = 1
            };

            var result = await _useCase.Execute(claim);

            _claimRepositoryMock.Verify(r => r.Add(It.IsAny<Claim>()), Times.Once);
            Assert.Equal("Título válido", result.Title);
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenTitleIsMissing()
        {
            var claim = new Claim { Title = "", Description = "ok", Priority = "Alta", Category = "General", User_id = 1 };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(claim));

            Assert.Equal("El título del reclamo es obligatorio", ex.Message);
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenDescriptionIsMissing()
        {
            var claim = new Claim { Title = "t", Description = "", Priority = "Alta", Category = "General", User_id = 1 };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(claim));

            Assert.Equal("La descripción del reclamo es obligatoria", ex.Message);
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenPriorityIsMissing()
        {
            var claim = new Claim { Title = "t", Description = "d", Priority = "", Category = "General", User_id = 1 };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(claim));

            Assert.Equal("La prioridad es obligatoria", ex.Message);
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenUserIdIsNull()
        {
            var claim = new Claim { Title = "t", Description = "d", Priority = "Alta", Category = "General", User_id = null };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(claim));

            Assert.Equal("Debe asociarse un usuario al reclamo", ex.Message);
        }
    }
}
