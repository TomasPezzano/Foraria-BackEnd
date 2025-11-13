using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Moq;

namespace ForariaTest.Unit;

public class UpdateUserDocumentTests
{
    private readonly Mock<IUserDocumentRepository> _documentRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateUserDocument _useCase;

    public UpdateUserDocumentTests()
    {
        _documentRepoMock = new Mock<IUserDocumentRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _useCase = new UpdateUserDocument(
            _documentRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object
        );
    }

    // 1. Documento no existe
    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenDocumentDoesNotExist()
    {
        int documentId = 1;
        int userId = 5;

        _documentRepoMock
            .Setup(r => r.GetById(documentId))
            .ReturnsAsync((UserDocument?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.ExecuteAsync(documentId, userId, null, null, null, null));

        Assert.Equal($"El documento con ID {documentId} no existe.", ex.Message);
    }

    // 2. Usuario no existe
    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        int documentId = 1;
        int userId = 5;

        _documentRepoMock
            .Setup(r => r.GetById(documentId))
            .ReturnsAsync(new UserDocument { Id = documentId });

        _userRepoMock
            .Setup(r => r.GetById(userId))
            .ReturnsAsync((User?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.ExecuteAsync(documentId, userId, null, null, null, null));

        Assert.Equal($"El usuario con ID {userId} no existe.", ex.Message);
    }

    // 3. Usuario sin permisos
    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorized_WhenUserHasNoPermissions()
    {
        int documentId = 1;
        int userId = 10;

        var document = new UserDocument
        {
            Id = documentId,
            User_id = 999 // otro dueño
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Vecino" }
        };

        _documentRepoMock.Setup(r => r.GetById(documentId)).ReturnsAsync(document);
        _userRepoMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _useCase.ExecuteAsync(documentId, userId, null, null, null, null));

        Assert.Equal("No tiene permisos para modificar este documento.", ex.Message);
    }

    // 4. Categoría inválida
    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenCategoryIsInvalid()
    {
        int documentId = 1;
        int userId = 1;

        var document = new UserDocument
        {
            Id = documentId,
            User_id = userId,
            Category = "Inexistente"
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Administrador" }
        };

        _documentRepoMock.Setup(r => r.GetById(documentId)).ReturnsAsync(document);
        _userRepoMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _useCase.ExecuteAsync(documentId, userId, null, null, "CategoriaInvalida", null));

        Assert.Equal("La categoría del documento no es válida.", ex.Message);
    }

    // 5. URL inválida (regex falla)
    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenUrlIsInvalid()
    {
        int documentId = 1;
        int userId = 1;

        var document = new UserDocument
        {
            Id = documentId,
            User_id = userId,
            Url = "archivo.pdf"
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Administrador" }
        };

        _documentRepoMock.Setup(r => r.GetById(documentId)).ReturnsAsync(document);
        _userRepoMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _useCase.ExecuteAsync(documentId, userId, null, null, null, "URL_MAL"));

        Assert.Equal("La URL del documento no es válida.", ex.Message);
    }

    // 6. Extensión inválida
    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenExtensionIsInvalid()
    {
        int documentId = 1;
        int userId = 1;

        var document = new UserDocument
        {
            Id = documentId,
            User_id = userId,
            Url = "foto.exe" // EXTENSION NO PERMITIDA
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Administrador" }
        };

        _documentRepoMock.Setup(r => r.GetById(documentId)).ReturnsAsync(document);
        _userRepoMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _useCase.ExecuteAsync(documentId, userId, null, null, null, "https://dominio.com/foto.exe"));

        Assert.StartsWith("El formato del documento no es válido", ex.Message);
    }

    // 7. Actualización exitosa
    [Fact]
    public async Task ExecuteAsync_ShouldUpdateDocument_WhenDataIsValid()
    {
        int documentId = 1;
        int userId = 1;

        var document = new UserDocument
        {
            Id = documentId,
            User_id = userId,
            Category = "Contrato",
            Url = "archivo.pdf"
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Administrador" }
        };

        _documentRepoMock.Setup(r => r.GetById(documentId)).ReturnsAsync(document);
        _userRepoMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        string newTitle = "Nuevo título";
        string newUrl = "https://dominio.com/archivo.pdf";

        var result = await _useCase.ExecuteAsync(documentId, userId, newTitle, null, null, newUrl);

        Assert.Equal(newTitle, document.Title);
        Assert.Equal(newUrl, document.Url);

        _documentRepoMock.Verify(r => r.Update(document), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // 8. Admin puede modificar aunque no sea dueño
    [Fact]
    public async Task ExecuteAsync_ShouldAllowAdminToModify_WhenNotOwner()
    {
        int documentId = 1;
        int userId = 10;

        var document = new UserDocument
        {
            Id = documentId,
            User_id = 999,
            Category = "Contrato",
            Url = "archivo.pdf"
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Administrador" }
        };

        _documentRepoMock.Setup(r => r.GetById(documentId)).ReturnsAsync(document);
        _userRepoMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var result = await _useCase.ExecuteAsync(documentId, userId, "T", null, null, null);

        Assert.Equal("T", document.Title);
    }

    // 9. Consorcio puede modificar aunque no sea dueño
    [Fact]
    public async Task ExecuteAsync_ShouldAllowConsortiumToModify_WhenNotOwner()
    {
        int documentId = 1;
        int userId = 10;

        var document = new UserDocument
        {
            Id = documentId,
            User_id = 999,
            Category = "Contrato",
            Url = "archivo.pdf"
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Consorcio" }
        };

        _documentRepoMock.Setup(r => r.GetById(documentId)).ReturnsAsync(document);
        _userRepoMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var result = await _useCase.ExecuteAsync(documentId, userId, "T", null, null, null);

        Assert.Equal("T", document.Title);
    }
}
