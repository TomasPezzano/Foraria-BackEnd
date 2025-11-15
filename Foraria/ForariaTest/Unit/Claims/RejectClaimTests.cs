using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Claims
{
    public class RejectClaimTests
    {
        [Fact]
        public async Task Execute_ShouldThrowArgumentException_WhenClaimDoesNotExist()
        {
            int claimId = 1;
            var repositoryMock = new Mock<IClaimRepository>();
            repositoryMock.Setup(r => r.GetById(claimId)).ReturnsAsync((Claim)null);

            var useCase = new RejectClaim(repositoryMock.Object);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => useCase.Execute(claimId));
            Assert.Equal($"No se encontró un reclamo con ID {claimId}", ex.Message);
        }

        [Fact]
        public async Task Execute_ShouldThrowInvalidOperationException_WhenClaimAlreadyRejected()
        {
            int claimId = 2;
            var claim = new Claim { Id = claimId, State = "Rechazado" };

            var repositoryMock = new Mock<IClaimRepository>();
            repositoryMock.Setup(r => r.GetById(claimId)).ReturnsAsync(claim);

            var useCase = new RejectClaim(repositoryMock.Object);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.Execute(claimId));
            Assert.Equal($"El reclamo con ID {claimId} ya está rechazado", ex.Message);
        }

        [Fact]
        public async Task Execute_ShouldRejectClaim_WhenClaimIsValid()
        {
            int claimId = 3;
            var claim = new Claim { Id = claimId, State = "Pendiente" };

            var repositoryMock = new Mock<IClaimRepository>();
            repositoryMock.Setup(r => r.GetById(claimId)).ReturnsAsync(claim);
            repositoryMock.Setup(r => r.Update(claim)).Returns(Task.CompletedTask);

            var useCase = new RejectClaim(repositoryMock.Object);

            await useCase.Execute(claimId);

            Assert.Equal("Rechazado", claim.State);
            repositoryMock.Verify(r => r.Update(claim), Times.Once);
        }
    }
}
