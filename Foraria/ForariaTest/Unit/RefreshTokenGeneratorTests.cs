using Xunit;
using Foraria.Application.UseCase;

namespace ForariaTest.Unit;

public class RefreshTokenGeneratorTests
{
    [Fact]
    public void Generate_ShouldReturnNonEmptyString()
    {
        // Arrange
        var generator = new RefreshTokenGenerator();

        // Act
        var token = generator.Generate();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void Generate_ShouldReturnCorrectLength()
    {
        // Arrange
        var generator = new RefreshTokenGenerator();

        // Act
        var token = generator.Generate();

        // Assert
        // 64 bytes en Base64 = 88 caracteres
        Assert.Equal(88, token.Length);
    }

    [Fact]
    public void Generate_MultipleCalls_ShouldReturnDifferentTokens()
    {
        // Arrange
        var generator = new RefreshTokenGenerator();

        // Act
        var token1 = generator.Generate();
        var token2 = generator.Generate();
        var token3 = generator.Generate();

        // Assert
        Assert.NotEqual(token1, token2);
        Assert.NotEqual(token2, token3);
        Assert.NotEqual(token1, token3);
    }

    [Fact]
    public void Generate_100Tokens_ShouldAllBeUnique()
    {
        // Arrange
        var generator = new RefreshTokenGenerator();
        var tokens = new HashSet<string>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            tokens.Add(generator.Generate());
        }

        // Assert
        Assert.Equal(100, tokens.Count); // Todos únicos
    }

    [Fact]
    public void Generate_TokenShouldBeBase64()
    {
        // Arrange
        var generator = new RefreshTokenGenerator();

        // Act
        var token = generator.Generate();

        // Assert
        // Intentar convertir de Base64 no debe lanzar excepción
        var bytes = Convert.FromBase64String(token);
        Assert.Equal(64, bytes.Length); // 64 bytes originales
    }
}