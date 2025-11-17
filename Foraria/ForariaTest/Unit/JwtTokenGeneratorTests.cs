using Foraria.Domain.Repository;
using ForariaDomain.Aplication.Configuration;
using ForariaDomain.Application.UseCase;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ForariaTest.Unit;

public class JwtTokenGeneratorTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly GenerateJwtToken _jwtGenerator;

    public JwtTokenGeneratorTests()
    {
        // Mock del UserRepository
        _mockUserRepo = new Mock<IUserRepository>();

        // Configurar JwtSettings
        var jwtSettings = new JwtSettings
        {
            SecretKey = "0123456789ABCDEF0123456789ABCDEF",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 60
        };

        var optionsMock = new Mock<IOptions<JwtSettings>>();
        optionsMock.Setup(o => o.Value).Returns(jwtSettings);

        // Crear el generador con el mock del UserRepository
        _jwtGenerator = new GenerateJwtToken(optionsMock.Object, _mockUserRepo.Object);
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

        // Mock: Usuario tiene acceso al consorcio 1
        _mockUserRepo
            .Setup(r => r.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int> { 1 });

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
        int userId = 1;
        _mockUserRepo
            .Setup(r => r.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int> { 1 });

        // Act
        string token = _jwtGenerator.Generate(userId, "a@a.com", 1, "Admin", false, true);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert JTI exists
        Assert.NotNull(jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti));
    }

    [Fact]
    public void Generate_WithMultipleConsortiums_ShouldIncludeAllConsortiumIds()
    {
        // Arrange
        int userId = 5;
        var consortiumIds = new List<int> { 1, 2, 3 };

        _mockUserRepo
            .Setup(r => r.GetConsortiumIdsByUserId(userId))
            .Returns(consortiumIds);

        // Act
        string token = _jwtGenerator.Generate(userId, "admin@test.com", 2, "Administrador", false, true);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert: Debe tener 3 claims de consortiumId
        var consortiumClaims = jwt.Claims
            .Where(c => c.Type == "consortiumId")
            .Select(c => int.Parse(c.Value))
            .ToList();

        Assert.Equal(3, consortiumClaims.Count);
        Assert.Contains(1, consortiumClaims);
        Assert.Contains(2, consortiumClaims);
        Assert.Contains(3, consortiumClaims);
    }

    [Fact]
    public void Generate_WithNoConsortiums_ShouldNotIncludeConsortiumIdClaim()
    {
        // Arrange
        int userId = 7;
        _mockUserRepo
            .Setup(r => r.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int>()); // Usuario sin consorcios

        // Act
        string token = _jwtGenerator.Generate(userId, "test@test.com", 1, "SomeRole", false, true);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert: No debe tener claims de consortiumId
        var consortiumClaims = jwt.Claims.Where(c => c.Type == "consortiumId");
        Assert.Empty(consortiumClaims);
    }

    [Fact]
    public void Generate_WithSingleConsortium_ShouldIncludeOneConsortiumIdClaim()
    {
        // Arrange
        int userId = 8;
        _mockUserRepo
            .Setup(r => r.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int> { 42 });

        // Act
        string token = _jwtGenerator.Generate(userId, "propietario@test.com", 3, "Propietario", false, true);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert: Debe tener exactamente 1 claim de consortiumId con valor 42
        var consortiumClaims = jwt.Claims
            .Where(c => c.Type == "consortiumId")
            .Select(c => int.Parse(c.Value))
            .ToList();

        Assert.Single(consortiumClaims);
        Assert.Equal(42, consortiumClaims.First());
    }
}