using Foraria.Application.UseCase;
using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Moq;
using Thread = ForariaDomain.Thread;

namespace ForariaTest.Application
{
    public class CreateThreadTests
    {
        private readonly Mock<IThreadRepository> _mockThreadRepo;
        private readonly Mock<IForumRepository> _mockForumRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly CreateThread _useCase;

        public CreateThreadTests()
        {
            _mockThreadRepo = new Mock<IThreadRepository>();
            _mockForumRepo = new Mock<IForumRepository>();
            _mockUserRepo = new Mock<IUserRepository>();

            _useCase = new CreateThread(
                _mockThreadRepo.Object,
                _mockForumRepo.Object,
                _mockUserRepo.Object
            );
        }

        [Fact]
        public async Task GivenValidRequest_WhenCreatingThread_ThenThreadIsCreatedSuccessfully()
        {
            // Given
            var request = new CreateThreadRequest
            {
                Theme = "Reclamos Generales",
                Description = "Discusión sobre reclamos del consorcio",
                Forum_id = 1,
                User_id = 10
            };

            var mockForum = new Forum { Id = 1, Category = ForumCategory.General, Threads = new List<Thread>() };
            var mockUser = new User { Id = 10, Name = "Admin" };

            _mockForumRepo.Setup(r => r.GetById(request.Forum_id)).ReturnsAsync(mockForum);
            _mockUserRepo.Setup(r => r.GetById(request.User_id)).ReturnsAsync(mockUser);
            _mockThreadRepo.Setup(r => r.Add(It.IsAny<Thread>())).Returns(Task.CompletedTask);

            // When
            var result = await _useCase.Execute(request);

            // Then
            Assert.NotNull(result);
            Assert.Equal(request.Theme, result.Theme);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Forum_id, result.Forum_id);
            Assert.Equal(request.User_id, result.User_id);

            _mockForumRepo.Verify(r => r.GetById(request.Forum_id), Times.Once);
            _mockUserRepo.Verify(r => r.GetById(request.User_id), Times.Once);
            _mockThreadRepo.Verify(r => r.Add(It.IsAny<Thread>()), Times.Once);
        }

        [Fact]
        public async Task GivenInvalidForum_WhenCreatingThread_ThenThrowsException()
        {
            // Given
            var request = new CreateThreadRequest
            {
                Theme = "Nuevo Tema",
                Description = "Descripción de prueba",
                Forum_id = 99,
                User_id = 1
            };

            _mockForumRepo.Setup(r => r.GetById(request.Forum_id)).ReturnsAsync((Forum?)null);

            // When / Then
            await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(request));
        }

        [Fact]
        public async Task GivenDuplicateThemeInForum_WhenCreatingThread_ThenThrowsException()
        {
            // Given
            var request = new CreateThreadRequest
            {
                Theme = "Duplicado",
                Description = "Intentando duplicar tema",
                Forum_id = 1,
                User_id = 1
            };

            var existingThread = new Thread { Id = 2, Theme = "Duplicado", Forum_id = 1 };
            var mockForum = new Forum
            {
                Id = 1,
                Category = ForumCategory.General,
                Threads = new List<Thread> { existingThread }
            };
            var mockUser = new User { Id = 1, Name = "TestUser" };

            _mockForumRepo.Setup(r => r.GetById(request.Forum_id)).ReturnsAsync(mockForum);
            _mockUserRepo.Setup(r => r.GetById(request.User_id)).ReturnsAsync(mockUser);

            // When / Then
            await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(request));

            _mockThreadRepo.Verify(r => r.Add(It.IsAny<Thread>()), Times.Never);
        }
    }
}
