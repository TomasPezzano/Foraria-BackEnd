using System.Collections.Generic;
using System.Threading.Tasks;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Moq;
using Xunit;

namespace ForariaTest.Unit
{
    public class TransferPermissionTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private readonly Mock<IResidenceRepository> _residenceRepositoryMock;
        private readonly TransferPermission _useCase;

        public TransferPermissionTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _residenceRepositoryMock = new Mock<IResidenceRepository>();

            _useCase = new TransferPermission(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object,
                _residenceRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Execute_WhenOwnerNotFound_ThrowsNotFoundException()
        {
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1))
                .ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(1, 2));
            Assert.Equal("Propietario no encontrado", ex.Message);
        }

        [Fact]
        public async Task Execute_WhenTenantNotFound_ThrowsNotFoundException()
        {
            var owner = CreateUser(1, "Propietario");
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1)).ReturnsAsync(owner);
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(2)).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(1, 2));
            Assert.Equal("Inquilino no encontrado", ex.Message);
        }

        [Fact]
        public async Task Execute_WhenOwnerIsNotPropietario_ThrowsBusinessException()
        {
            var owner = CreateUser(1, "Administrador");
            var tenant = CreateUser(2, "Inquilino");

            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1)).ReturnsAsync(owner);
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(2)).ReturnsAsync(tenant);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _useCase.Execute(1, 2));
            Assert.Equal("Solo los propietarios pueden transferir permisos", ex.Message);
        }

        [Fact]
        public async Task Execute_WhenTenantIsNotInquilino_ThrowsBusinessException()
        {
            var owner = CreateUser(1, "Propietario");
            var tenant = CreateUser(2, "Administrador");

            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1)).ReturnsAsync(owner);
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(2)).ReturnsAsync(tenant);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _useCase.Execute(1, 2));
            Assert.Equal("Los permisos solo pueden transferirse a inquilinos", ex.Message);
        }

        [Fact]
        public async Task Execute_WhenNoSharedResidences_ThrowsBusinessException()
        {
            var owner = CreateUser(1, "Propietario");
            var tenant = CreateUser(2, "Inquilino");

            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1)).ReturnsAsync(owner);
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(2)).ReturnsAsync(tenant);

            _residenceRepositoryMock.Setup(r => r.GetResidenceByUserId(1))
                .ReturnsAsync(new List<Residence> { new Residence { Id = 1 } });

            _residenceRepositoryMock.Setup(r => r.GetResidenceByUserId(2))
                .ReturnsAsync(new List<Residence> { new Residence { Id = 2 } });

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _useCase.Execute(1, 2));
            Assert.Equal("El propietario y el inquilino deben compartir al menos una residencia", ex.Message);
        }

        [Fact]
        public async Task Execute_WhenValid_TransfersPermissionsCorrectly()
        {
            var owner = CreateUser(1, "Propietario", hasPermission: true);
            var tenant = CreateUser(2, "Inquilino");

            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1)).ReturnsAsync(owner);
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(2)).ReturnsAsync(tenant);

            var sharedResidence = new Residence { Id = 1 };
            _residenceRepositoryMock.Setup(r => r.GetResidenceByUserId(1))
                .ReturnsAsync(new List<Residence> { sharedResidence });
            _residenceRepositoryMock.Setup(r => r.GetResidenceByUserId(2))
                .ReturnsAsync(new List<Residence> { sharedResidence });

            _userRepositoryMock.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);
            _refreshTokenRepositoryMock.Setup(r => r.RevokeAllByUserId(2)).Returns(Task.CompletedTask);

            await _useCase.Execute(1, 2);

            Assert.False(owner.HasPermission);
            Assert.True(tenant.HasPermission);
            _userRepositoryMock.Verify(r => r.Update(owner), Times.Once);
            _userRepositoryMock.Verify(r => r.Update(tenant), Times.Once);
            _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserId(2), Times.Once);
        }

        private User CreateUser(int id, string roleDesc, bool hasPermission = false)
        {
            return new User
            {
                Id = id,
                Name = $"User{id}",
                Mail = $"user{id}@test.com",
                HasPermission = hasPermission,
                Role = new Role { Id = 1, Description = roleDesc },
                Residences = new List<Residence>()
            };
        }
    }
}
