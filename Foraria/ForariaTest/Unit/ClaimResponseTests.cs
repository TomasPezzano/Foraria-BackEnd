using Foraria.Application.UseCase;
using Foraria.Interface.Controllers;
using Foraria.Interface.DTOs;
using ForariaDomain.Application.UseCase;
using ForariaDomain;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class ClaimResponseTests
{
    private readonly Mock<ICreateClaimResponse> _createClaimResponseMock;
    private readonly Mock<IGetUserById> _getUserByIdMock;
    private readonly Mock<IGetClaimById> _getClaimByIdMock;
    private readonly Mock<IGetResponsibleSectorById> _getSectorByIdMock;
    private readonly ClaimResponseController _controller;

    public ClaimResponseTests()
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
    public async Task Add_ValidDto_ReturnsOkWithResponse()
    {
        var dto = new ClaimResponseDto
        {
            Description = "Respuesta al reclamo",
            ResponseDate = DateTime.UtcNow,
            User_id = 1,
            Claim_id = 2,
            ResponsibleSector_id = 3
        };

        var user = new User { Id = dto.User_id };
        var claim = new Claim { Id = dto.Claim_id };
        var sector = new ResponsibleSector { Id = dto.ResponsibleSector_id };

        var expectedResponse = new ClaimResponseDto
        {
            Description = dto.Description,
            ResponseDate = dto.ResponseDate,
            User_id = dto.User_id,
            Claim_id = dto.Claim_id,
            ResponsibleSector_id = dto.ResponsibleSector_id
        };

        _getUserByIdMock.Setup(x => x.Execute(dto.User_id)).ReturnsAsync(user);
        _getClaimByIdMock.Setup(x => x.Execute(dto.Claim_id)).ReturnsAsync(claim);
        _getSectorByIdMock.Setup(x => x.Execute(dto.ResponsibleSector_id)).ReturnsAsync(sector);
        _createClaimResponseMock.Setup(x => x.Execute(It.IsAny<ClaimResponse>())).ReturnsAsync(expectedResponse);

        var result = await _controller.Add(dto);

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
        var dto = new ClaimResponseDto();
        _controller.ModelState.AddModelError("Description", "La descripción es obligatoria.");

        var result = await _controller.Add(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Add_ThrowsArgumentException_ReturnsBadRequestWithError()
    {
        var dto = new ClaimResponseDto
        {
            Description = "Inválido",
            ResponseDate = DateTime.UtcNow,
            User_id = 1,
            Claim_id = 2,
            ResponsibleSector_id = 3
        };

        var user = new User { Id = dto.User_id };
        var claim = new Claim { Id = dto.Claim_id };
        var sector = new ResponsibleSector { Id = dto.ResponsibleSector_id };

        _getUserByIdMock.Setup(x => x.Execute(dto.User_id)).ReturnsAsync(user);
        _getClaimByIdMock.Setup(x => x.Execute(dto.Claim_id)).ReturnsAsync(claim);
        _getSectorByIdMock.Setup(x => x.Execute(dto.ResponsibleSector_id)).ReturnsAsync(sector);
        _createClaimResponseMock.Setup(x => x.Execute(It.IsAny<ClaimResponse>()))
            .ThrowsAsync(new ArgumentException("La descripción es obligatoria."));

        var result = await _controller.Add(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var value = badRequest.Value;
        var errorProp = value.GetType().GetProperty("error");
        var error = errorProp?.GetValue(value)?.ToString();

        Assert.Equal("La descripción es obligatoria.", error);
    }

    [Fact]
    public async Task Add_ThrowsException_ReturnsInternalServerError()
    {
        var dto = new ClaimResponseDto
        {
            Description = "Respuesta",
            ResponseDate = DateTime.UtcNow,
            User_id = 1,
            Claim_id = 2,
            ResponsibleSector_id = 3
        };

        var user = new User { Id = dto.User_id };
        var claim = new Claim { Id = dto.Claim_id };
        var sector = new ResponsibleSector { Id = dto.ResponsibleSector_id };

        _getUserByIdMock.Setup(x => x.Execute(dto.User_id)).ReturnsAsync(user);
        _getClaimByIdMock.Setup(x => x.Execute(dto.Claim_id)).ReturnsAsync(claim);
        _getSectorByIdMock.Setup(x => x.Execute(dto.ResponsibleSector_id)).ReturnsAsync(sector);
        _createClaimResponseMock.Setup(x => x.Execute(It.IsAny<ClaimResponse>()))
            .ThrowsAsync(new Exception("Error inesperado"));

        var result = await _controller.Add(dto);

        var errorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, errorResult.StatusCode);

        var value = errorResult.Value;
        var errorProp = value.GetType().GetProperty("error");
        var detailsProp = value.GetType().GetProperty("details");

        Assert.Equal("Ocurrió un error interno", errorProp?.GetValue(value)?.ToString());
        Assert.Equal("Error inesperado", detailsProp?.GetValue(value)?.ToString());
    }
}
