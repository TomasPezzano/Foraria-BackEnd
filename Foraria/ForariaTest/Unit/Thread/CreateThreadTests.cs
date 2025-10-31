﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace ForariaTest.Unit.Thread
{
    public class CreateThreadTests
    {
        [Fact]
        public async Task Execute_ShouldCreateThread_WhenForumAndUserExist_AndThemeIsUnique()
        {
            // Arrange
            var request = new CreateThreadRequest
            {
                Theme = "Nuevo tema",
                Description = "Descripción del hilo",
                ForumId = 1,
                UserId = 1
            };

            var forum = new global::ForariaDomain.Forum
            {
                Id = 1,
                Threads = new List<global::ForariaDomain.Thread>()
            };

            var user = new global::ForariaDomain.User
            {
                Id = 1,
                Name = "TestUser"
            };

            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockForumRepo = new Mock<IForumRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockForumRepo.Setup(r => r.GetById(1)).ReturnsAsync(forum);
            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockThreadRepo.Setup(r => r.Add(It.IsAny<global::ForariaDomain.Thread>()))
                          .Returns(Task.CompletedTask);

            var useCase = new CreateThread(mockThreadRepo.Object, mockForumRepo.Object, mockUserRepo.Object);

            // Act
            var result = await useCase.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Theme.Should().Be("Nuevo tema");
            result.Description.Should().Be("Descripción del hilo");
            result.State.Should().Be("Active");
            result.Forum_id.Should().Be(1);
            result.User_id.Should().Be(1);

            mockThreadRepo.Verify(r => r.Add(It.IsAny<global::ForariaDomain.Thread>()), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenForumDoesNotExist()
        {
            var request = new CreateThreadRequest { ForumId = 10, UserId = 1, Theme = "Tema", Description = "Desc" };

            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockForumRepo = new Mock<IForumRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockForumRepo.Setup(r => r.GetById(10)).ReturnsAsync((global::ForariaDomain.Forum?)null);

            var useCase = new CreateThread(mockThreadRepo.Object, mockForumRepo.Object, mockUserRepo.Object);

            Func<Task> act = async () => await useCase.Execute(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El foro con ID 10 no existe.");
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenUserDoesNotExist()
        {
            var request = new CreateThreadRequest { ForumId = 1, UserId = 99, Theme = "Tema", Description = "Desc" };

            var forum = new global::ForariaDomain.Forum { Id = 1, Threads = new List<global::ForariaDomain.Thread>() };

            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockForumRepo = new Mock<IForumRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockForumRepo.Setup(r => r.GetById(1)).ReturnsAsync(forum);
            mockUserRepo.Setup(r => r.GetById(99)).ReturnsAsync((global::ForariaDomain.User?)null);

            var useCase = new CreateThread(mockThreadRepo.Object, mockForumRepo.Object, mockUserRepo.Object);

            Func<Task> act = async () => await useCase.Execute(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El usuario con ID 99 no existe.");
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenThemeAlreadyExistsInForum()
        {
            var request = new CreateThreadRequest
            {
                ForumId = 1,
                UserId = 1,
                Theme = "Tema repetido",
                Description = "Desc"
            };

            var existingThread = new global::ForariaDomain.Thread { Theme = "tema repetido" };

            var forum = new global::ForariaDomain.Forum
            {
                Id = 1,
                Threads = new List<global::ForariaDomain.Thread> { existingThread }
            };

            var user = new global::ForariaDomain.User { Id = 1 };

            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockForumRepo = new Mock<IForumRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockForumRepo.Setup(r => r.GetById(1)).ReturnsAsync(forum);
            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);

            var useCase = new CreateThread(mockThreadRepo.Object, mockForumRepo.Object, mockUserRepo.Object);

            Func<Task> act = async () => await useCase.Execute(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Ya existe un hilo con el título 'Tema repetido' en este foro.");
        }
    }
}
