using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using ForariaDomain;

namespace ForariaTest.Unit.Residences
{
    public class GetAllResidencesByConsortiumWithOwnerTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldReturnResidences_FromRepository()
        {
            var expectedResidences = new List<Residence>
            {
                new Residence { Id = 1, ConsortiumId = 5 },
                new Residence { Id = 2, ConsortiumId = 5 }
            };

            var residenceRepositoryMock = new Mock<IResidenceRepository>();

            residenceRepositoryMock
                .Setup(r => r.GetAllResidencesByConsortiumWithOwner())
                .ReturnsAsync(expectedResidences);

            var useCase = new GetAllResidencesByConsortiumWithOwner(residenceRepositoryMock.Object);

            var result = await useCase.ExecuteAsync();

            Assert.NotNull(result);
            Assert.Equal(expectedResidences, result);

            residenceRepositoryMock.Verify(
                r => r.GetAllResidencesByConsortiumWithOwner(),
                Times.Once
            );
        }
    }
}
