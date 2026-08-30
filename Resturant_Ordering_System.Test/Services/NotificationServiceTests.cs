using AutoMapper;
using Business_Layer.DTOs.NotificationDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Resturant_Ordering_System.Application.Services;
using Resturant_Ordering_System.Domain.Abstract;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<IGenaricRepo<AppUser, string>> _users = new();
        private readonly Mock<INotificationRepo> _notifications = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILogger<NotificationService>> _logger = new();
        private readonly NotificationService _sut;

        public NotificationServiceTests()
        {
            _uow.Setup(u => u.AppUserRepo).Returns(_users.Object);
            _uow.Setup(u => u.Notifications).Returns(_notifications.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<GetNotificationDto>>(_cache);
            _sut = new NotificationService(_uow.Object, _cache.Object, _mapper.Object, _logger.Object);
        }

        [Fact]
        public async Task CreateAsync_WhenUserExists_CreatesNotificationAndClearsCache()
        {
            var dto = new CreateNotificationDto { UserId = "u1", Title = "Hi", Message = "Msg" };
            var user = new AppUser { Id = "u1" };
            var entity = new Notifications { Id = 1, UserId = "u1" };
            var resultDto = new GetNotificationDto { Id = 1, UserId = "u1" };
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(user);
            _mapper.Setup(m => m.Map<Notifications>(dto)).Returns(entity);
            _mapper.Setup(m => m.Map<GetNotificationDto>(entity)).Returns(resultDto);

            var result = await _sut.CreateAsync(dto);

            Assert.Same(resultDto, result);
            _notifications.Verify(r => r.CreateAsync(entity), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Notifications"), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenUserIsMissing_ThrowsNotFoundException()
        {
            var dto = new CreateNotificationDto { UserId = "missing", Title = "Hi", Message = "Msg" };
            _users.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(dto));

            Assert.Equal("User not found", ex.Message);
            _notifications.Verify(r => r.CreateAsync(It.IsAny<Notifications>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_WhenCalled_ReturnsMappedPage()
        {
            _notifications.Setup(r => r.GetAllPaged(1, 10))
                .ReturnsAsync((new List<Notifications>().AsEnumerable(), 0));
            _mapper.Setup(m => m.Map<List<GetNotificationDto>>(It.IsAny<IEnumerable<Notifications>>()))
                .Returns(new List<GetNotificationDto>());

            var result = await _sut.GetAllAsync(1, 10);

            Assert.Empty(result.Data);
            Assert.Equal(1, result.PageNumber);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_WhenCalled_UsesUserQuery()
        {
            var query = new List<Notifications>().AsQueryable();
            _notifications.Setup(r => r.GetUserNotifications("u1")).Returns(query);
            _notifications.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Notifications>(), 0));
            _mapper.Setup(m => m.Map<List<GetNotificationDto>>(It.IsAny<IEnumerable<Notifications>>()))
                .Returns(new List<GetNotificationDto>());

            var result = await _sut.GetUserNotificationsAsync("u1", 1, 10);

            Assert.Empty(result.Data);
            _notifications.Verify(r => r.GetUserNotifications("u1"), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotificationExists_ReturnsMappedDto()
        {
            var entity = new Notifications { Id = 5 };
            var dto = new GetNotificationDto { Id = 5 };
            _notifications.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(entity);
            _mapper.Setup(m => m.Map<GetNotificationDto>(entity)).Returns(dto);

            var result = await _sut.GetByIdAsync(5);

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotificationIsMissing_ThrowsNotFoundException()
        {
            _notifications.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Notifications?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(5));

            Assert.Equal("Notification not found", ex.Message);
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenUserAndNotificationExist_SetsIsReadAndClearsCache()
        {
            var notification = new Notifications { Id = 2, IsRead = false };
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _notifications.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(notification);

            await _sut.MarkAsReadAsync(2, "u1");

            Assert.True(notification.IsRead);
            _notifications.Verify(r => r.UpdateAsync(notification), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Notifications"), Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenUserIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.MarkAsReadAsync(2, "u1"));

            Assert.Equal("User not found", ex.Message);
            _notifications.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenNotificationIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _notifications.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Notifications?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.MarkAsReadAsync(2, "u1"));

            Assert.Equal("Notification not found", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_WhenUserAndNotificationExist_DeletesAndClearsCache()
        {
            var notification = new Notifications { Id = 2 };
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _notifications.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(notification);

            await _sut.DeleteAsync(2, "u1");

            _notifications.Verify(r => r.DeleteAsync(notification), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Notifications"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenUserIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(2, "u1"));

            Assert.Equal("User not found", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotificationIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _notifications.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Notifications?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(2, "u1"));

            Assert.Equal("Notification not found", ex.Message);
        }
    }
}
