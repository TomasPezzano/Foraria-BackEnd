using Foraria.Application.UseCase;
using Foraria.Interface.Controllers;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class UserDocumentTests
{
    private readonly Mock<ICreateUserDocument> _createUserDocumentMock;
    private readonly Mock<IGetUserDocuments> _getUserDocumentsMock;
    private readonly UserDocumentController _controller;

    public UserDocumentTests()
    {
        _createUserDocumentMock = new Mock<ICreateUserDocument>();
        _getUserDocumentsMock = new Mock<IGetUserDocuments>();
        _controller = new UserDocumentController(_createUserDocumentMock.Object, _getUserDocumentsMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithListOfDocuments()
    {
        // Arrange
        var documents = new List<UserDocument>
        {
            new UserDocument { Id = 1, Title = "Doc 1", Category = "Legal", Url = "http://example.com", User_id = 1, Consortium_id = 1, CreatedAt = DateTime.UtcNow },
            new UserDocument { Id = 2, Title = "Doc 2", Category = "Finance", Url = "http://example.com", User_id = 2, Consortium_id = 2, CreatedAt = DateTime.UtcNow }
        };

        _getUserDocumentsMock.Setup(x => x.Execute()).ReturnsAsync(documents);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDocs = Assert.IsType<List<UserDocumentDto>>(okResult.Value);
        Assert.Equal(2, returnedDocs.Count);
    }

    [Fact]
    public async Task Add_ValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreateUserDocumentDto
        {
            Title = "New Doc",
            Category = "Legal",
            Url = "http://example.com",
            User_id = 1,
            Consortium_id = 1
        };

        var document = new UserDocument
        {
            Id = 99,
            Title = dto.Title,
            Category = dto.Category,
            Url = dto.Url,
            User_id = dto.User_id,
            Consortium_id = dto.Consortium_id,
            CreatedAt = DateTime.UtcNow
        };

        _createUserDocumentMock.Setup(x => x.Execute(It.IsAny<UserDocument>())).ReturnsAsync(document);

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedDto = Assert.IsType<UserDocumentDto>(createdResult.Value);
        Assert.Equal(99, returnedDto.Id);
        Assert.Equal(dto.Title, returnedDto.Title);
        Assert.Equal(dto.Category, returnedDto.Category);
    }

    [Fact]
    public async Task Add_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateUserDocumentDto(); // vacío
        _controller.ModelState.AddModelError("Title", "El título es obligatorio.");

        // Act
        var result = await _controller.Add(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Add_ThrowsArgumentException_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var dto = new CreateUserDocumentDto
        {
            Title = "", // inválido
            Category = "Legal",
            Url = "http://example.com",
            User_id = 1,
            Consortium_id = 1
        };

        _createUserDocumentMock.Setup(x => x.Execute(It.IsAny<UserDocument>()))
            .ThrowsAsync(new ArgumentException("El título del documento es obligatorio."));

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var value = badRequest.Value;
        var messageProp = value.GetType().GetProperty("message");
        var message = messageProp?.GetValue(value)?.ToString();

        Assert.Equal("El título del documento es obligatorio.", message);
    }
}
