using FluentAssertions;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using Moq;


namespace ForariaTest.Tests.UserDocuments
{
    public class UserDocumentTests
    {
        private readonly Mock<IUserDocumentRepository> _userDocRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IConsortiumRepository> _consortiumRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CreateUserDocument _createUserDocument;
        private readonly GetUserDocuments _getUserDocuments;

        public UserDocumentTests()
        {
            _userDocRepoMock = new Mock<IUserDocumentRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _consortiumRepoMock = new Mock<IConsortiumRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _createUserDocument = new CreateUserDocument(
                _userDocRepoMock.Object,
                _userRepoMock.Object,
                _consortiumRepoMock.Object,
                _unitOfWorkMock.Object
            );

            _getUserDocuments = new GetUserDocuments(_userDocRepoMock.Object);
        }

        [Fact]
        public async Task Execute_ShouldCreateDocument_WhenUserAndConsortiumExist()
        {
            // Arrange
            var document = new global::ForariaDomain.UserDocument
            {
                Title = "Contrato de alquiler",
                Category = "Contrato",
                Url = "https://ejemplo.com/contrato.pdf",
                User_id = 1,
                Consortium_id = 10
            };

            var user = new global::ForariaDomain.User
            {
                Id = 1,
                Role = new Role { Description = "Residente" }
            };
            var consortium = new global::ForariaDomain.Consortium { Id = 10, Name = "Consorcio Central" };

            _userRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(user);
            _consortiumRepoMock.Setup(r => r.FindById(10)).ReturnsAsync(consortium);
            _userDocRepoMock.Setup(r => r.Add(It.IsAny<global::ForariaDomain.UserDocument>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _createUserDocument.Execute(document);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.Title.Should().Be("Contrato de alquiler");
            _userDocRepoMock.Verify(r => r.Add(It.IsAny<global::ForariaDomain.UserDocument>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var document = new global::ForariaDomain.UserDocument
            {
                Title = "Documento sin usuario",
                Category = "Contrato",
                Url = "https://ejemplo.com/contrato.pdf",
                User_id = 999,
                Consortium_id = 1
            };

            _userRepoMock.Setup(r => r.GetById(999))
                         .ReturnsAsync((global::ForariaDomain.User?)null);

            // Act
            Func<Task> act = async () => await _createUserDocument.Execute(document);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("El usuario con ID 999 no existe.");
        }

        [Fact]
        public async Task Execute_ShouldThrowNotFound_WhenConsortiumDoesNotExist()
        {
            // Arrange
            var document = new global::ForariaDomain.UserDocument
            {
                Title = "Documento inválido",
                Category = "Contrato",
                Url = "https://ejemplo.com/contrato.pdf",
                User_id = 1,
                Consortium_id = 500
            };

            var user = new global::ForariaDomain.User
            {
                Id = 1,
                Role = new Role { Description = "Residente" }
            };

            _userRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(user);
            _consortiumRepoMock.Setup(r => r.FindById(500))
                               .ReturnsAsync((global::ForariaDomain.Consortium?)null);

            // Act
            Func<Task> act = async () => await _createUserDocument.Execute(document);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("El consorcio con ID 500 no existe.");
        }

        [Fact]
        public async Task Execute_ShouldThrowArgumentException_WhenUrlIsInvalid()
        {
            // Arrange
            var document = new global::ForariaDomain.UserDocument
            {
                Title = "Documento con URL inválida",
                Category = "Contrato",
                Url = "archivo_local.pdf",
                User_id = 1,
                Consortium_id = 1
            };

            var user = new global::ForariaDomain.User
            {
                Id = 1,
                Role = new Role { Description = "Residente" }
            };
            var consortium = new global::ForariaDomain.Consortium { Id = 1 };

            _userRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(user);
            _consortiumRepoMock.Setup(r => r.FindById(1)).ReturnsAsync(consortium);

            // Act
            Func<Task> act = async () => await _createUserDocument.Execute(document);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("La URL del documento no es válida.");
        }

        [Fact]
        public async Task Execute_ShouldReturnListOfDocuments()
        {
            // Arrange
            var docs = new List<global::ForariaDomain.UserDocument>
            {
                new global::ForariaDomain.UserDocument { Id = 1, Title = "Doc1", Category = "Contrato", Url = "https://ex.com/doc1.pdf" },
                new global::ForariaDomain.UserDocument { Id = 2, Title = "Doc2", Category = "Actas", Url = "https://ex.com/doc2.pdf" }
            };

            _userDocRepoMock.Setup(r => r.GetAll()).ReturnsAsync(docs);

            // Act
            var result = await _getUserDocuments.Execute();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Title.Should().Be("Doc1");
            _userDocRepoMock.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldReturnEmptyList_WhenNoDocumentsExist()
        {
            // Arrange
            _userDocRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<global::ForariaDomain.UserDocument>());

            // Act
            var result = await _getUserDocuments.Execute();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Execute_ShouldThrowArgumentException_WhenCategoryIsInvalid()
        {
            var document = new global::ForariaDomain.UserDocument
            {
                Title = "Documento con categoría inválida",
                Category = "CategoriaInexistente",
                Url = "https://ejemplo.com/doc.pdf",
                User_id = 1,
                Consortium_id = 1
            };

            var user = new global::ForariaDomain.User { Id = 1 };
            var consortium = new global::ForariaDomain.Consortium { Id = 1 };

            _userRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(user);
            _consortiumRepoMock.Setup(r => r.FindById(1)).ReturnsAsync(consortium);

            Func<Task> act = async () => await _createUserDocument.Execute(document);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("La categoría del documento no es válida.");
        }


        [Fact]
        public async Task Execute_ShouldThrowArgumentException_WhenDocumentExtensionIsInvalid()
        {
            var document = new global::ForariaDomain.UserDocument
            {
                Title = "Documento con extensión inválida",
                Category = "Contrato",
                Url = "https://ejemplo.com/archivo.exe",  
                User_id = 1,
                Consortium_id = 1
            };

            var user = new global::ForariaDomain.User { Id = 1 };
            var consortium = new global::ForariaDomain.Consortium { Id = 1 };

            _userRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(user);
            _consortiumRepoMock.Setup(r => r.FindById(1)).ReturnsAsync(consortium);

            Func<Task> act = async () => await _createUserDocument.Execute(document);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("El formato del documento no es válido (solo .pdf, .jpg, .png, docx, txt, xls, xlsx, csv, ppt, pptx y odt).");
        }

    }
}
