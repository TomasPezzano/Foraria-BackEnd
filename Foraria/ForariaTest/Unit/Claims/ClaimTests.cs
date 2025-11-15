using Foraria.Application.Services;
using Foraria.Controllers;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class ClaimTests
{
    private readonly Mock<ICreateClaim> _createClaimMock;
    private readonly Mock<IGetClaims> _getClaimsMock;
    private readonly Mock<IRejectClaim> _rejectClaimMock;
    private readonly Mock<IFileProcessor> _fileProcessorMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly ClaimController _controller;

    public ClaimTests()
    {
        _createClaimMock = new Mock<ICreateClaim>();
        _getClaimsMock = new Mock<IGetClaims>();
        _rejectClaimMock = new Mock<IRejectClaim>();
        _fileProcessorMock = new Mock<IFileProcessor>();
        _permissionServiceMock = new Mock<IPermissionService>();
        _controller = new ClaimController(_createClaimMock.Object, _getClaimsMock.Object, _rejectClaimMock.Object, _fileProcessorMock.Object, _permissionServiceMock.Object);
    }

    [Fact]
    public async Task Add_ValidClaimDtoWithArchive_ReturnsCreatedAtAction()
    {
        var dto = new ClaimDto
        {
            Title = "Nuevo reclamo",
            Description = "Descripción",
            Priority = "Media",
            Category = "Servicios",
            Archive = "data:image/png;base64,VALIDBASE64==",
            User_id = 1,
            ResidenceId = 2
        };

        var claim = new Claim
        {
            Id = 99,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Category = dto.Category,
            Archive = "claims/archivo.png",
            User_id = dto.User_id,
            ResidenceId = dto.ResidenceId,
            CreatedAt = DateTime.UtcNow,
            State = "Nuevo"
        };

        _fileProcessorMock.Setup(fp => fp.SaveBase64FileAsync(dto.Archive, "claims"))
            .ReturnsAsync("claims/archivo.png");

        _createClaimMock.Setup(x => x.Execute(It.IsAny<Claim>()))
            .ReturnsAsync(claim);

        var result = await _controller.Add(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(99, created.RouteValues["id"]);

        var response = created.Value;
        var archiveProp = response.GetType().GetProperty("ArchiveUrl");
        var archiveUrl = archiveProp?.GetValue(response)?.ToString();

        Assert.Equal("claims/archivo.png", archiveUrl);
    }

    [Fact]
    public async Task Add_InvalidBase64_ThrowsDomainValidationException()
    {
        var dto = new ClaimDto
        {
            Title = "Reclamo",
            Description = "Descripción",
            Priority = "Alta",
            Category = "General",
            Archive = "data:image/png;base64,estoNoEsBase64",
            User_id = 1,
            ResidenceId = 2
        };

        _fileProcessorMock.Setup(fp => fp.SaveBase64FileAsync(dto.Archive, "claims"))
            .ThrowsAsync(new FormatException());

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => _controller.Add(dto));

        Assert.Equal("El formato del archivo Base64 no es válido.", ex.Message);
    }
}
