using ForariaDomain.Aplication.Configuration;
using ForariaDomain.Application.UseCase;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ForariaTest.Unit;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _jwtGenerator;

    public JwtTokenGeneratorTests()
    {
        var jwtSettings = new JwtSettings
        {
            SecretKey = "0123456789ABCDEF0123456789ABCDEF", 
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 60
        };

        var optionsMock = new Mock<IOptions<JwtSettings>>();
        optionsMock.Setup(o => o.Value).Returns(jwtSettings);

        _jwtGenerator = new JwtTokenGenerator(optionsMock.Object);
    }

    [Fact]
    public void Generate_ShouldReturnValidJwtToken()
    {
        // Arrange
        int userId = 10;
        string email = "test@test.com";
        int roleId = 2;
        string roleName = "Admin";
        bool requiresPasswordChange = true;
        bool? hasPermission = false;

        // Act
        string token = _jwtGenerator.Generate(userId, email, roleId, roleName, requiresPasswordChange, hasPermission);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));

        // Decode token
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Validate claims
        Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(roleName, jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal(roleId.ToString(), jwt.Claims.First(c => c.Type == "roleId").Value);
        Assert.Equal(hasPermission.ToString(), jwt.Claims.First(c => c.Type == "hasPermission").Value);
        Assert.Equal(requiresPasswordChange.ToString(), jwt.Claims.First(c => c.Type == "requiresPasswordChange").Value);

        // Validate issuer and audience
        Assert.Equal("TestIssuer", jwt.Issuer);
        Assert.Equal("TestAudience", jwt.Audiences.First());

        // Validate expiration time
        Assert.True(jwt.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public void Generate_ShouldIncludeJtiClaim()
    {
        // Arrange
        string token = _jwtGenerator.Generate(1, "a@a.com", 1, "Admin", false, true);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert JTI exists
        Assert.NotNull(jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti));
    }
}
