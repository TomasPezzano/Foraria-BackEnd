using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Users
{
    public class GetUserByIdTests
    {
        [Fact]
        public async Task Execute_ShouldReturnUser_WhenUserExists()
        {
            var userId = 1;
            var expectedUser = new User { Id = userId, Name = "Test User", Mail = "test@example.com" };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(repo => repo.GetById(userId))
                .ReturnsAsync(expectedUser);

            var getUserById = new GetUserById(userRepositoryMock.Object);

            var result = await getUserById.Execute(userId);

            Assert.NotNull(result);
            Assert.Equal(expectedUser.Id, result.Id);
            Assert.Equal(expectedUser.Name, result.Name);
            Assert.Equal(expectedUser.Mail, result.Mail);
        }

        [Fact]
        public async Task Execute_ShouldThrowInvalidOperationException_WhenUserDoesNotExist()
        {
            var userId = 99;

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(repo => repo.GetById(userId))
                .ReturnsAsync((User)null); 

            var getUserById = new GetUserById(userRepositoryMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                getUserById.Execute(userId));

            Assert.Equal($"User with id {userId} not found.", exception.Message);
        }
    }
}
