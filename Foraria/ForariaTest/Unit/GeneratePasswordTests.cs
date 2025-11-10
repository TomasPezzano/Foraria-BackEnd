using ForariaDomain.Application.UseCase;

namespace ForariaTest;

public class GeneratePasswordTests
{
    private readonly IGeneratePassword _generatePasswordUseCase;

    public GeneratePasswordTests()
    {
        _generatePasswordUseCase = new GeneratePassword();
    }


    [Fact]
    public async Task Generate_ShouldReturnPasswordWithCorrectLength()
    {
        const int expectedLength = 10;

        var password = await _generatePasswordUseCase.Generate();

        Assert.Equal(expectedLength, password.Length);
    }


    [Fact]
    public async Task Generate_ShouldContainUpperCase()
    {
        var password = await _generatePasswordUseCase.Generate();

        Assert.Contains(password, char.IsUpper);
    }

 
    [Fact]
    public async Task Generate_ShouldContainLowerCase()
    {
        var password = await _generatePasswordUseCase.Generate();

        Assert.Contains(password, char.IsLower);
    }


    [Fact]
    public async Task Generate_ShouldContainDigit()
    {
        var password = await _generatePasswordUseCase.Generate();

        Assert.Contains(password, char.IsDigit);
    }


    [Fact]
    public async Task Generate_ShouldContainSpecialCharacter()
    {
        var specialChars = "!@#$%^&*()-_=+";

        var password = await _generatePasswordUseCase.Generate();

        Assert.True(password.Any(c => specialChars.Contains(c)),
            "Password should contain at least one special character");
    }

 
    [Fact]
    public async Task Generate_MultipleCalls_ShouldReturnDifferentPasswords()
    {
        var password1 = await _generatePasswordUseCase.Generate();
        var password2 = await _generatePasswordUseCase.Generate();
        var password3 = await _generatePasswordUseCase.Generate();
       
        Assert.NotEqual(password1, password2);
        Assert.NotEqual(password2, password3);
        Assert.NotEqual(password1, password3);
    }
}