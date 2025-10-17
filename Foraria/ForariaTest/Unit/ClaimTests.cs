using Foraria.Application.UseCase;
using Foraria.Interface.Controllers;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ForariaTest.Unit;

public class ClaimTests
{
    private readonly Mock<ICreateClaim> _createClaimMock;
    private readonly Mock<IGetClaims> _getClaimsMock;
    private readonly Mock<IRejectClaim> _rejectClaimMock;
    private readonly ClaimController _controller;

    public ClaimTests()
    {
        _createClaimMock = new Mock<ICreateClaim>();
        _getClaimsMock = new Mock<IGetClaims>();
        _rejectClaimMock = new Mock<IRejectClaim>();
        _controller = new ClaimController(_createClaimMock.Object, _getClaimsMock.Object, _rejectClaimMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithMappedClaims()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim
            {
                Title = "Reclamo 1",
                Description = "Descripción",
                Priority = "Alta",
                Category = "General",
                Archive = null,
                User_id = 1,
                ClaimResponse = new ClaimResponse
                {
                    Description = "Respuesta",
                    ResponseDate = DateTime.UtcNow,
                    ResponsibleSector_id = 2,
                    User = new User { Id = 1 },
                    Claim = new Claim { Id = 10 }
                }
            }
        };

        _getClaimsMock.Setup(x => x.Execute()).ReturnsAsync(claims);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task Add_ValidClaimDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new ClaimDto
        {
            Title = "Nuevo reclamo",
            Description = "Descripción",
            Priority = "Media",
            Category = "Servicios",
            Archive = null,
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
            Archive = null,
            User_id = dto.User_id,
            ResidenceId = dto.ResidenceId,
            CreatedAt = DateTime.UtcNow,
            State = "Nuevo"
        };

        _createClaimMock.Setup(x => x.Execute(It.IsAny<Claim>())).ReturnsAsync(claim);

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(99, created.RouteValues["id"]);
    }

    [Fact]
    public async Task Add_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ClaimDto(); // vacío
        _controller.ModelState.AddModelError("Title", "El título es obligatorio.");

        // Act
        var result = await _controller.Add(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Add_ThrowsFormatException_ReturnsBadRequest()
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

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var value = badRequest.Value;
        var messageProp = value.GetType().GetProperty("message");
        var message = messageProp?.GetValue(value)?.ToString();

        Assert.Equal("El formato del archivo Base64 no es válido.", message);
    }

    [Fact]
    public async Task Add_ThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var dto = new ClaimDto
        {
            Title = "Reclamo",
            Description = "Descripción",
            Priority = "Alta",
            Category = "General",
            Archive = null,
            User_id = 1,
            ResidenceId = 2
        };

        _createClaimMock.Setup(x => x.Execute(It.IsAny<Claim>()))
            .ThrowsAsync(new Exception("Error inesperado"));

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, errorResult.StatusCode);

        var value = errorResult.Value;
        var messageProp = value.GetType().GetProperty("message");
        var detailsProp = value.GetType().GetProperty("details");

        Assert.Equal("Error interno del servidor", messageProp?.GetValue(value)?.ToString());
        Assert.Equal("Error inesperado", detailsProp?.GetValue(value)?.ToString());
    }

    [Fact]
    public async Task RejectClaimById_ValidId_ReturnsOkWithMessage()
    {
        // Arrange
        int claimId = 5;
        _rejectClaimMock.Setup(x => x.Execute(claimId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RejectClaimById(claimId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value;
        var messageProp = value.GetType().GetProperty("message");
        var message = messageProp?.GetValue(value)?.ToString();

        Assert.Equal($"El reclamo con ID {claimId} fue rechazado correctamente", message);
    }

    [Fact]
    public async Task RejectClaimById_ThrowsArgumentException_ReturnsNotFound()
    {
        // Arrange
        int claimId = 99;
        _rejectClaimMock.Setup(x => x.Execute(claimId))
            .ThrowsAsync(new ArgumentException("No se encontró el reclamo"));

        // Act
        var result = await _controller.RejectClaimById(claimId);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var value = notFound.Value;
        var errorProp = value.GetType().GetProperty("error");
        var error = errorProp?.GetValue(value)?.ToString();

        Assert.Equal("No se encontró el reclamo", error);
    }

    [Fact]
    public async Task RejectClaimById_ThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        int claimId = 77;
        _rejectClaimMock.Setup(x => x.Execute(claimId))
            .ThrowsAsync(new Exception("Error inesperado"));

        // Act
        var result = await _controller.RejectClaimById(claimId);

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
