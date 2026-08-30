using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System.Security.Claims;

namespace Resturant_Ordering_System.Test.Helpers
{
    public static class TestMockHelpers
    {
        public static Mock<UserManager<AppUser>> CreateUserManager()
        {
            var store = new Mock<IUserStore<AppUser>>();
            return new Mock<UserManager<AppUser>>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);
        }

        public static Mock<IDbContextTransaction> CreateTransaction()
        {
            var transaction = new Mock<IDbContextTransaction>();
            transaction
                .Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            transaction
                .Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            transaction
                .Setup(t => t.DisposeAsync())
                .Returns(ValueTask.CompletedTask);
            return transaction;
        }

        public static void SetupCacheToExecuteFactory<T>(Mock<ICacheService> cache)
        {
            cache.Setup(c => c.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<T?>>>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>()))
                .Returns((string _, Func<Task<T?>> factory, TimeSpan? _, TimeSpan? _) => factory());
        }

        public static Mock<IFormFile> CreateFormFile(
            string fileName = "photo.png",
            string contentType = "image/png")
        {
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.ContentType).Returns(contentType);
            return file;
        }

        public static void SetUser(this ControllerBase controller, string? userId)
        {
            var claims = string.IsNullOrEmpty(userId)
                ? Array.Empty<Claim>()
                : new[] { new Claim(ClaimTypes.NameIdentifier, userId) };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            };
        }
    }
}
