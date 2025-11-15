using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using ForariaDomain;
using Moq;

namespace ForariaTest.Unit.Users
{
    public class LogoutUserTests
    {
        [Fact]
        public async Task Logout_ShouldThrowNotFoundException_WhenTokenDoesNotExist()
        {
            string refreshToken = "invalid-token";
            string ipAddress = "127.0.0.1";

            var repositoryMock = new Mock<IRefreshTokenRepository>();
            repositoryMock.Setup(r => r.GetByToken(refreshToken)).ReturnsAsync((RefreshToken)null);

            var logoutUser = new LogoutUser(repositoryMock.Object);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => logoutUser.Logout(refreshToken, ipAddress));
            Assert.Equal("Invalid refresh token", ex.Message);
        }

        [Fact]
        public async Task Logout_ShouldRevokeToken_WhenTokenIsActive()
        {
            string refreshToken = "valid-token";
            string ipAddress = "127.0.0.1";

            var token = new RefreshToken
            {
                Id = 1,
                Token = refreshToken,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedByIp = "127.0.0.1"
            };

            var repositoryMock = new Mock<IRefreshTokenRepository>();
            repositoryMock.Setup(r => r.GetByToken(refreshToken)).ReturnsAsync(token);
            repositoryMock.Setup(r => r.Update(token)).Returns(Task.CompletedTask);

            var logoutUser = new LogoutUser(repositoryMock.Object);

            await logoutUser.Logout(refreshToken, ipAddress);

            Assert.True(token.IsRevoked);
            Assert.Equal(ipAddress, token.RevokedByIp);
            Assert.NotNull(token.RevokedAt);
            repositoryMock.Verify(r => r.Update(token), Times.Once);
        }

        [Fact]
        public async Task Logout_ShouldDoNothing_WhenTokenIsInactive()
        {
           
            string refreshToken = "inactive-token";
            string ipAddress = "127.0.0.1";

            var token = new RefreshToken
            {
                Id = 2,
                Token = refreshToken,
                IsRevoked = true, 
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddHours(-1), 
                CreatedByIp = "127.0.0.1"
            };

            var repositoryMock = new Mock<IRefreshTokenRepository>();
            repositoryMock.Setup(r => r.GetByToken(refreshToken)).ReturnsAsync(token);

            var logoutUser = new LogoutUser(repositoryMock.Object);

            await logoutUser.Logout(refreshToken, ipAddress);

            Assert.True(token.IsRevoked); 
            Assert.Null(token.RevokedByIp); 
            Assert.Null(token.RevokedAt);   
            repositoryMock.Verify(r => r.Update(It.IsAny<RefreshToken>()), Times.Never);
        }
    }
}
