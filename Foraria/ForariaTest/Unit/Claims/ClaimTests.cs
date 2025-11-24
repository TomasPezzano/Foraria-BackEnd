using Foraria.Application.Services;
using Foraria.Controllers;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Foraria.Domain.Repository;

public class ClaimTests
{
    private readonly Mock<ICreateClaim> _createClaimMock;
    private readonly Mock<IGetClaims> _getClaimsMock;
    private readonly Mock<IRejectClaim> _rejectClaimMock;
    private readonly Mock<IFileProcessor> _fileProcessorMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;

    // 👉 FIX IMPORTANTE
    private readonly Mock<IClaimRepository> _claimRepositoryMock;
    private readonly GetPendingClaimsCount _getPendingClaimsCount;

    private readonly ClaimController _controller;

    public ClaimTests()
    {
        _createClaimMock = new Mock<ICreateClaim>();
        _getClaimsMock = new Mock<IGetClaims>();
        _rejectClaimMock = new Mock<IRejectClaim>();
        _fileProcessorMock = new Mock<IFileProcessor>();
        _permissionServiceMock = new Mock<IPermissionService>();

        // 👉 Se mockea el repositorio real
        _claimRepositoryMock = new Mock<IClaimRepository>();

        // 👉 Se instancia el caso de uso REAL
        _getPendingClaimsCount = new GetPendingClaimsCount(_claimRepositoryMock.Object);

        // Permisos siempre permitidos en tests
        _permissionServiceMock
            .Setup(p => p.EnsurePermissionAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Crear el controller
        _controller = new ClaimController(
            _createClaimMock.Object,
            _getClaimsMock.Object,
            _rejectClaimMock.Object,
            _fileProcessorMock.Object,
            _permissionServiceMock.Object,
            _getPendingClaimsCount
        );

        // Usuario vacío para el HttpContext
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }



    // ======================================================
    //                 TEST: ADD OK
    // ======================================================
    [Fact]
    public async Task Add_ValidClaimDtoWithArchive_ReturnsOkObjectResult()
    {
        // Arrange
        var dto = new ClaimDto
        {
            Title = "Nuevo reclamo",
            Description = "Descripción",
            Priority = "Media",
            Category = "Servicios",
            Archive = "data:image/png;base64,VALIDBASE64==",
            User_id = 1,
            ResidenceId = 2,
            ConsortiumId = 9
        };

        var createdClaim = new ForariaDomain.Claim
        {
            Id = 99,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Category = dto.Category,
            Archive = "claims/archivo.png",
            User_id = dto.User_id,
            ResidenceId = dto.ResidenceId,
            ConsortiumId = dto.ConsortiumId,
            CreatedAt = DateTime.Now,
            State = "Nuevo"
        };

        _fileProcessorMock
            .Setup(fp => fp.SaveBase64FileAsync(dto.Archive, "claims"))
            .ReturnsAsync("claims/archivo.png");

        _createClaimMock
            .Setup(x => x.Execute(It.IsAny<ForariaDomain.Claim>()))
            .ReturnsAsync(createdClaim);

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value;

        var dict = value.GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(value));

        Assert.Equal(99, dict["Id"]);
        Assert.Equal("claims/archivo.png", dict["ArchiveUrl"]);
    }



    // ======================================================
    //        TEST: BASE64 INVÁLIDO (FormatException)
    // ======================================================
    [Fact]
    public async Task Add_InvalidBase64_ThrowsDomainValidationException()
    {
        var dto = new ClaimDto
        {
            Title = "Reclamo",
            Description = "Descripción",
            Priority = "Alta",
            Category = "General",
            Archive = "data:image/png;base64,ESTONOESBASE64",
            User_id = 1,
            ResidenceId = 2,
            ConsortiumId = 3
        };

        _fileProcessorMock
            .Setup(fp => fp.SaveBase64FileAsync(dto.Archive, "claims"))
            .ThrowsAsync(new FormatException());

        // Act + Assert
        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => _controller.Add(dto));

        Assert.Equal("El formato del archivo Base64 no es válido.", ex.Message);
    }
}
