using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Foraria.Application.UseCase;
using Foraria.Interface.Controllers;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ForariaTest.Unit;

public class ClaimResponseControllerTests
{
    private readonly Mock<ICreateClaimResponse> _createClaimResponseMock;
    private readonly ClaimResponseController _controller;

    public ClaimResponseControllerTests()
    {
        _createClaimResponseMock = new Mock<ICreateClaimResponse>();
        _controller = new ClaimResponseController(_createClaimResponseMock.Object);
    }

    [Fact]
    public async Task Add_ValidDto_ReturnsOkWithResponse()
    {
        // Arrange
        var dto = new ClaimResponseDto
        {
            Description = "Respuesta al reclamo",
            ResponseDate = DateTime.UtcNow,
            User_id = 1,
            Claim_id = 2,
            ResponsibleSector_id = 3
        };

        var expectedResponse = new ClaimResponseDto
        {
            Description = dto.Description,
            ResponseDate = dto.ResponseDate,
            User_id = dto.User_id,
            Claim_id = dto.Claim_id,
            ResponsibleSector_id = dto.ResponsibleSector_id
        };

        _createClaimResponseMock.Setup(x => x.Execute(dto)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<ClaimResponseDto>(okResult.Value);
        Assert.Equal(dto.Description, returned.Description);
        Assert.Equal(dto.User_id, returned.User_id);
        Assert.Equal(dto.Claim_id, returned.Claim_id);
        Assert.Equal(dto.ResponsibleSector_id, returned.ResponsibleSector_id);
    }

    [Fact]
    public async Task Add_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ClaimResponseDto(); // vacío
        _controller.ModelState.AddModelError("Description", "La descripción es obligatoria.");

        // Act
        var result = await _controller.Add(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Add_ThrowsArgumentException_ReturnsBadRequestWithError()
    {
        // Arrange
        var dto = new ClaimResponseDto
        {
            Description = "", // inválido
            ResponseDate = DateTime.UtcNow,
            User_id = 1,
            Claim_id = 2,
            ResponsibleSector_id = 3
        };

        _createClaimResponseMock.Setup(x => x.Execute(dto))
            .ThrowsAsync(new ArgumentException("La descripción es obligatoria."));

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var value = badRequest.Value;
        var errorProp = value.GetType().GetProperty("error");
        var error = errorProp?.GetValue(value)?.ToString();

        Assert.Equal("La descripción es obligatoria.", error);
    }

    [Fact]
    public async Task Add_ThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var dto = new ClaimResponseDto
        {
            Description = "Respuesta",
            ResponseDate = DateTime.UtcNow,
            User_id = 1,
            Claim_id = 2,
            ResponsibleSector_id = 3
        };

        _createClaimResponseMock.Setup(x => x.Execute(dto))
            .ThrowsAsync(new Exception("Error inesperado"));

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, errorResult.StatusCode);

        var value = errorResult.Value;
        var errorProp = value.GetType().GetProperty("error");
        var detailsProp = value.GetType().GetProperty("details");

        Assert.Equal("Ocurrió un error interno", errorProp?.GetValue(value)?.ToString());
        Assert.Equal("Error inesperado", detailsProp?.GetValue(value)?.ToString());
    }
}
