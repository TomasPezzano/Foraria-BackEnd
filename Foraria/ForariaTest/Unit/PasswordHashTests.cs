using Xunit;
using Foraria.Application.UseCase;

namespace ForariaTest;

public class PasswordHashTests
{
    private readonly IPasswordHash _passwordHashUseCase;

    public PasswordHashTests()
    {
        _passwordHashUseCase = new HashPassword();
    }


    [Fact]
    public void HashPassword_SamePassword_ShouldReturnDifferentHashes()
    {
        // ARRANGE
        var password = "MySecurePassword123!";

        // ACT
        var hash1 = _passwordHashUseCase.Execute(password);
        var hash2 = _passwordHashUseCase.Execute(password);

        // ASSERT
        Assert.NotEqual(hash1, hash2); 
        Assert.NotEqual(password, hash1);
        Assert.StartsWith("$2a$", hash1); // BCrypt siempre empieza con $2a$
    }


    [Fact]
    public void VerifyPassword_CorrectPassword_ShouldReturnTrue()
    {
        var password = "MySecurePassword123!";
        var hash = _passwordHashUseCase.Execute(password);

        var result = _passwordHashUseCase.VerifyPassword(password, hash);

        Assert.True(result);
    }


    [Fact]
    public void VerifyPassword_IncorrectPassword_ShouldReturnFalse()
    {
        var correctPassword = "MySecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _passwordHashUseCase.Execute(correctPassword);

        var result = _passwordHashUseCase.VerifyPassword(wrongPassword, hash);

        Assert.False(result);
    }


    [Fact]
    public void HashPassword_EmptyString_ShouldNotThrowException()
    {
        var password = "";

        var hash = _passwordHashUseCase.Execute(password);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }
}