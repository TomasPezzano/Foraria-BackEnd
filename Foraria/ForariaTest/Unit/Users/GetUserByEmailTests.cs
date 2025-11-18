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
    public class GetUserByEmailTests
    {
        [Fact]
        public async Task Execute_ShouldReturnUser_WhenUserExists()
        {
            var email = "test@example.com";
            var expectedUser = new User { Mail = email, Name = "Test User" };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(repo => repo.GetByEmail(email))
                .ReturnsAsync(expectedUser);

            var getUserByEmail = new GetUserByEmail(userRepositoryMock.Object);

            var result = await getUserByEmail.Execute(email);

            Assert.NotNull(result);
            Assert.Equal(expectedUser.Mail, result.Mail);
            Assert.Equal(expectedUser.Name, result.Name);
        }

        [Fact]
        public async Task Execute_ShouldThrowInvalidOperationException_WhenUserDoesNotExist()
        {
            var email = "nonexistent@example.com";

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(repo => repo.GetByEmail(email))
                .ReturnsAsync((User)null); 

            var getUserByEmail = new GetUserByEmail(userRepositoryMock.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                getUserByEmail.Execute(email));

            Assert.Equal($"User with email '{email}' not found.", exception.Message);
        }
    }
}
