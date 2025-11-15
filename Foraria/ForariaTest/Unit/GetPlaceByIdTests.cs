using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit
{
    public class GetPlaceByIdTests
    {
        private readonly Mock<IPlaceRepository> _mockRepository;
        private readonly GetPlaceById _useCase;

        public GetPlaceByIdTests()
        {
            _mockRepository = new Mock<IPlaceRepository>();
            _useCase = new GetPlaceById(_mockRepository.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnPlace_WhenPlaceExists()
        {
            int placeId = 10;
            var expectedPlace = new Place
            {
                Id = placeId,
                Name = "SUM",
                Reserves = new List<Reserve>()
            };

            _mockRepository
                .Setup(r => r.GetById(placeId))
                .ReturnsAsync(expectedPlace);

            var result = await _useCase.Execute(placeId);

            Assert.NotNull(result);
            Assert.Equal(expectedPlace.Id, result!.Id);
            Assert.Equal(expectedPlace.Name, result.Name);
            Assert.Equal(expectedPlace.Reserves, result.Reserves);

            _mockRepository.Verify(r => r.GetById(placeId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldReturnNull_WhenPlaceDoesNotExist()
        {
            int placeId = 99;

            _mockRepository
                .Setup(r => r.GetById(placeId))
                .ReturnsAsync((Place?)null);

            var result = await _useCase.Execute(placeId);

            Assert.Null(result);

            _mockRepository.Verify(r => r.GetById(placeId), Times.Once);
        }
    }
}
