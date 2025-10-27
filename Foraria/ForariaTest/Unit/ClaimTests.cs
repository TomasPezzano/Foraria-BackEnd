﻿using Foraria.Application.UseCase;
using Foraria.Interface.Controllers;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class ClaimTests
{
    private readonly Mock<ICreateClaim> _createClaimMock;
    private readonly Mock<IGetClaims> _getClaimsMock;
    private readonly Mock<IRejectClaim> _rejectClaimMock;
    private readonly Mock<IFileProcessor> _fileProcessorMock;
    private readonly ClaimController _controller;

    public ClaimTests()
    {
        _createClaimMock = new Mock<ICreateClaim>();
        _getClaimsMock = new Mock<IGetClaims>();
        _rejectClaimMock = new Mock<IRejectClaim>();
        _fileProcessorMock = new Mock<IFileProcessor>();
        _controller = new ClaimController(_createClaimMock.Object, _getClaimsMock.Object, _rejectClaimMock.Object, _fileProcessorMock.Object);
    }

    [Fact]
    public async Task Add_ValidClaimDtoWithArchive_ReturnsCreatedAtAction()
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

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(99, created.RouteValues["id"]);

        var response = created.Value;
        var archiveProp = response.GetType().GetProperty("ArchiveUrl");
        var archiveUrl = archiveProp?.GetValue(response)?.ToString();

        Assert.Equal("claims/archivo.png", archiveUrl);
    }

    [Fact]
    public async Task Add_InvalidBase64_ReturnsBadRequest()
    {
        // Arrange
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

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var value = badRequest.Value;
        var messageProp = value.GetType().GetProperty("message");
        var message = messageProp?.GetValue(value)?.ToString();

        Assert.Equal("El formato del archivo Base64 no es válido.", message);
    }
}
