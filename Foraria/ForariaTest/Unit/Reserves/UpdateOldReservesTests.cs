using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using ForariaDomain;
using Moq;

namespace ForariaTest.Unit.Reserves
{
    public class UpdateOldReservesTests
    {
        [Fact]
        public async Task Execute_ShouldNotCallUpdateRange_WhenNoOldReserves()
        {
           
            var reserves = new List<Reserve>
            {
                new Reserve { Id = 1, DeletedAt = DateTime.Now.AddHours(1), State = "Activo" },
                new Reserve { Id = 2, DeletedAt = null, State = "Activo" }
            };

            var repositoryMock = new Mock<IReserveRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(reserves);

            var useCase = new UpdateOldReserves(repositoryMock.Object);

            await useCase.Execute();

            repositoryMock.Verify(r => r.UpdateRange(It.IsAny<List<Reserve>>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldUpdateOldReservesAndCallUpdateRange()
        {
            var pastDate = DateTime.Now.AddHours(-1);
            var reserves = new List<Reserve>
            {
                new Reserve { Id = 1, DeletedAt = pastDate, State = "Activo" },
                new Reserve { Id = 2, DeletedAt = pastDate, State = "Activo" },
                new Reserve { Id = 3, DeletedAt = null, State = "Activo" },
            };

            var repositoryMock = new Mock<IReserveRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(reserves);
            repositoryMock.Setup(r => r.UpdateRange(It.IsAny<List<Reserve>>())).Returns(Task.CompletedTask).Verifiable();

            var useCase = new UpdateOldReserves(repositoryMock.Object);

            await useCase.Execute();

            var updatedReserves = reserves.Where(r => r.DeletedAt.HasValue && r.DeletedAt <= DateTime.Now).ToList();
            Assert.All(updatedReserves, r => Assert.Equal("Viejo", r.State));
            repositoryMock.Verify(r => r.UpdateRange(It.Is<List<Reserve>>(l => l.Count == 2)), Times.Once);
        }
    }
}
