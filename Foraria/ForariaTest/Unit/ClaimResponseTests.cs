using Foraria.Application.UseCase;
using Foraria.Controllers;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ForariaTest.Unit.ClaimResponseTests;

public class ClaimResponseControllerTests
{
    private readonly Mock<ICreateClaimResponse> _createClaimResponseMock;
    private readonly Mock<IGetUserById> _getUserByIdMock;
    private readonly Mock<IGetClaimById> _getClaimByIdMock;
    private readonly Mock<IGetResponsibleSectorById> _getSectorByIdMock;
    private readonly ClaimResponseController _controller;

    public ClaimResponseControllerTests()
    {
        _createClaimResponseMock = new Mock<ICreateClaimResponse>();
        _getUserByIdMock = new Mock<IGetUserById>();
        _getClaimByIdMock = new Mock<IGetClaimById>();
        _getSectorByIdMock = new Mock<IGetResponsibleSectorById>();

        _controller = new ClaimResponseController(
            _createClaimResponseMock.Object,
            _getUserByIdMock.Object,
            _getClaimByIdMock.Object,
            _getSectorByIdMock.Object
        );
    }

    [Fact]
    public async Task Add_ValidData_ReturnsCreatedAtActionWithResult()
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

        var user = new User { Id = dto.User_id, Name = "Juan" };
        var claim = new Claim { Id = dto.Claim_id, Title = "Reclamo A" };
        var sector = new ResponsibleSector { Id = dto.ResponsibleSector_id, Name = "Atención al cliente" };

        _getUserByIdMock.Setup(x => x.Execute(dto.User_id)).ReturnsAsync(user);
        _getClaimByIdMock.Setup(x => x.Execute(dto.Claim_id)).ReturnsAsync(claim);
        _getSectorByIdMock.Setup(x => x.Execute(dto.ResponsibleSector_id)).ReturnsAsync(sector);

        var expectedResult = new ClaimResponsResult
        {
            Id = 10,
            Description = dto.Description,
            ResponseDate = dto.ResponseDate,
            User_id = dto.User_id,
            Claim_id = dto.Claim_id,
            ResponsibleSector_id = dto.ResponsibleSector_id
        };

        _createClaimResponseMock
            .Setup(x => x.Execute(It.IsAny<ClaimResponse>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Add(dto);

        // Assert
        // Tolerar tanto CreatedAtActionResult como ObjectResult
        if (result is CreatedAtActionResult created)
        {
            var returned = created.Value;
            Assert.NotNull(returned);
        }
        else if (result is ObjectResult obj)
        {
            // Esto ayuda a ver qué pasó si no fue CreatedAtActionResult
            throw new Exception($"Expected CreatedAtActionResult, but got ObjectResult with status {obj.StatusCode}");
        }
        else
        {
            throw new Exception($"Unexpected result type: {result.GetType().Name}");
        }
    }


    [Fact]
    public async Task Add_InvalidModel_ReturnsBadRequest()
    {
        var dto = new ClaimResponseDto();
        _controller.ModelState.AddModelError("Description", "Campo requerido");

        var result = await _controller.Add(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Add_UserNotFound_ReturnsNotFound()
    {
        var dto = new ClaimResponseDto { User_id = 1, Claim_id = 2, ResponsibleSector_id = 3 };
        _getUserByIdMock.Setup(x => x.Execute(dto.User_id)).ReturnsAsync((User)null);

        var result = await _controller.Add(dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Usuario no encontrado", notFound.Value?.GetType().GetProperty("error")?.GetValue(notFound.Value));
    }

    [Fact]
    public async Task Add_ClaimNotFound_ReturnsNotFound()
    {
        var dto = new ClaimResponseDto { User_id = 1, Claim_id = 2, ResponsibleSector_id = 3 };
        _getUserByIdMock.Setup(x => x.Execute(dto.User_id)).ReturnsAsync(new User());
        _getClaimByIdMock.Setup(x => x.Execute(dto.Claim_id)).ReturnsAsync((Claim)null);

        var result = await _controller.Add(dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Reclamo no encontrado", notFound.Value?.GetType().GetProperty("error")?.GetValue(notFound.Value));
    }

    [Fact]
    public async Task Add_SectorNotFound_ReturnsNotFound()
    {
        var dto = new ClaimResponseDto { User_id = 1, Claim_id = 2, ResponsibleSector_id = 3 };
        _getUserByIdMock.Setup(x => x.Execute(dto.User_id)).ReturnsAsync(new User());
        _getClaimByIdMock.Setup(x => x.Execute(dto.Claim_id)).ReturnsAsync(new Claim());
        _getSectorByIdMock.Setup(x => x.Execute(dto.ResponsibleSector_id)).ReturnsAsync((ResponsibleSector)null);

        var result = await _controller.Add(dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Sector responsable no encontrado", notFound.Value?.GetType().GetProperty("error")?.GetValue(notFound.Value));
    }

    [Fact]
    public async Task Add_ThrowsArgumentException_ReturnsBadRequest()
    {
        var dto = new ClaimResponseDto { Description = "Inválido", User_id = 1, Claim_id = 2, ResponsibleSector_id = 3 };
        _getUserByIdMock.Setup(x => x.Execute(dto.User_id)).ReturnsAsync(new User());
        _getClaimByIdMock.Setup(x => x.Execute(dto.Claim_id)).ReturnsAsync(new Claim());
        _getSectorByIdMock.Setup(x => x.Execute(dto.ResponsibleSector_id)).ReturnsAsync(new ResponsibleSector());
        _createClaimResponseMock.Setup(x => x.Execute(It.IsAny<ClaimResponse>()))
            .ThrowsAsync(new ArgumentException("Falta descripción"));

        var result = await _controller.Add(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Falta descripción", badRequest.Value?.GetType().GetProperty("error")?.GetValue(badRequest.Value));
    }

    [Fact]
    public async Task Add_ThrowsException_ReturnsInternalServerError()
    {
        var dto = new ClaimResponseDto { Description = "Error", User_id = 1, Claim_id = 2, ResponsibleSector_id = 3 };
        _getUserByIdMock.Setup(x => x.Execute(dto.User_id)).ReturnsAsync(new User());
        _getClaimByIdMock.Setup(x => x.Execute(dto.Claim_id)).ReturnsAsync(new Claim());
        _getSectorByIdMock.Setup(x => x.Execute(dto.ResponsibleSector_id)).ReturnsAsync(new ResponsibleSector());
        _createClaimResponseMock.Setup(x => x.Execute(It.IsAny<ClaimResponse>()))
            .ThrowsAsync(new Exception("Error inesperado"));

        var result = await _controller.Add(dto);

        var errorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Ocurrió un error interno", errorResult.Value?.GetType().GetProperty("error")?.GetValue(errorResult.Value));
        Assert.Equal("Error inesperado", errorResult.Value?.GetType().GetProperty("details")?.GetValue(errorResult.Value));
    }
}
