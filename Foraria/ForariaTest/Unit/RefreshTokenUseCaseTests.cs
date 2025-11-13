using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;

using Moq;

namespace ForariaTest.Unit;

public class RefreshTokenUseCaseTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock;
    private readonly RefreshTokenUseCase _useCase;

    public RefreshTokenUseCaseTests()
    {
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();

        _useCase = new RefreshTokenUseCase(
            _refreshTokenRepoMock.Object,
            _jwtTokenGeneratorMock.Object,
            _refreshTokenGeneratorMock.Object
        );
    }

    // 1. Token no existe
    [Fact]
    public async Task Refresh_ShouldReturnFailure_WhenTokenDoesNotExist()
    {
        string refreshToken = "abc123";

        _refreshTokenRepoMock
            .Setup(r => r.GetByToken(refreshToken))
            .ReturnsAsync((ForariaDomain.RefreshToken?)null);

        var result = await _useCase.Refresh(refreshToken, "127.0.0.1");

        Assert.False(result.Success);
        Assert.Equal("Invalid refresh token", result.Message);
    }

    // 2. Token revocado
    [Fact]
    public async Task Refresh_ShouldReturnFailure_WhenTokenIsRevoked()
    {
        var token = new ForariaDomain.RefreshToken
        {
            Token = "abc123",
            IsRevoked = true
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByToken("abc123"))
            .ReturnsAsync(token);

        var result = await _useCase.Refresh("abc123", "127.0.0.1");

        Assert.False(result.Success);
        Assert.Equal("Token has been revoked", result.Message);
    }

    // 3. Token expirado
    [Fact]
    public async Task Refresh_ShouldReturnFailure_WhenTokenIsExpired()
    {
        var token = new ForariaDomain.RefreshToken
        {
            Token = "abc123",
            IsRevoked = false
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByToken("abc123"))
            .ReturnsAsync(token);

        var result = await _useCase.Refresh("abc123", "127.0.0.1");

        Assert.False(result.Success);
        Assert.Equal("Token has expired", result.Message);
    }
}
