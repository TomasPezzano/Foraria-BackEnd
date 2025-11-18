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
    public class RevokePermissionTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private readonly RevokePermission _useCase;

        public RevokePermissionTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _useCase = new RevokePermission(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object);
        }

        [Fact]
        public async Task Execute_WhenOwnerOrTenantNotFound_ThrowsNotFoundException()
        {
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1))
                .ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(1, 2));
            Assert.Equal("Usuario no encontrado", ex.Message);
        }

        [Fact]
        public async Task Execute_WhenOwnerIsNotPropietario_ThrowsBusinessException()
        {
            var owner = CreateUser(1, "Administrador");
            var tenant = CreateUser(2, "Inquilino", hasPermission: false);

            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1)).ReturnsAsync(owner);
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(2)).ReturnsAsync(tenant);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _useCase.Execute(1, 2));
            Assert.Equal("El usuario especificado no es un Propietario", ex.Message);
        }

        [Fact]
        public async Task Execute_WhenTenantIsNotInquilino_ThrowsBusinessException()
        {
            var owner = CreateUser(1, "Propietario");
            var tenant = CreateUser(2, "Propietario", hasPermission: false);

            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1)).ReturnsAsync(owner);
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(2)).ReturnsAsync(tenant);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _useCase.Execute(1, 2));
            Assert.Equal("El usuario especificado no es un Inquilino", ex.Message);
        }

        [Fact]
        public async Task Execute_WhenTenantHasActivePermission_ThrowsBusinessException()
        {
            var owner = CreateUser(1, "Propietario");
            var tenant = CreateUser(2, "Inquilino", hasPermission: true);

            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1)).ReturnsAsync(owner);
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(2)).ReturnsAsync(tenant);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _useCase.Execute(1, 2));
            Assert.Equal("El inquilino no tiene permisos activos para revocar", ex.Message);
        }

        [Fact]
        public async Task Execute_WhenAllValidationsPass_RevokesPermissionSuccessfully()
        {
            var owner = CreateUser(1, "Propietario", hasPermission: false);
            var tenant = CreateUser(2, "Inquilino", hasPermission: false);

            _userRepositoryMock.Setup(r => r.GetByIdWithRole(1)).ReturnsAsync(owner);
            _userRepositoryMock.Setup(r => r.GetByIdWithRole(2)).ReturnsAsync(tenant);

            _userRepositoryMock.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);
            _refreshTokenRepositoryMock.Setup(r => r.RevokeAllByUserId(2)).Returns(Task.CompletedTask);

            await _useCase.Execute(1, 2);

            Assert.True(owner.HasPermission);
            Assert.False(tenant.HasPermission);

            _userRepositoryMock.Verify(r => r.Update(owner), Times.Once);
            _userRepositoryMock.Verify(r => r.Update(tenant), Times.Once);
            _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserId(2), Times.Once);
        }

        private User CreateUser(int id, string roleDescription, bool hasPermission = false)
        {
            return new User
            {
                Id = id,
                Name = $"User{id}",
                Mail = $"user{id}@test.com",
                HasPermission = hasPermission,
                Role = new Role { Id = 1, Description = roleDescription },
                Residences = new List<Residence>()
            };
        }
    }
}
