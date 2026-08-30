using AutoMapper;
using Business_Layer.DTOs.NotificationDTOs;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using Resturant_Ordering_System.Application.DTOs.ReviewDTOs;
using Resturant_Ordering_System.Application.Interfaces.IService;
using Resturant_Ordering_System.Application.Services;
using Resturant_Ordering_System.Domain.Abstract;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Services
{
    public class ReviewServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<IReviewRepo> _reviews = new();
        private readonly Mock<IGenaricRepo<AppUser, string>> _users = new();
        private readonly Mock<IMenuItemRepo> _menuItems = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILogger<ReviewService>> _logger = new();
        private readonly Mock<INotificationService> _notifications = new();
        private readonly Mock<ISendNotificationService> _sendNotifications = new();
        private readonly Mock<IDbContextTransaction> _transaction;
        private readonly ReviewService _sut;

        public ReviewServiceTests()
        {
            _transaction = TestMockHelpers.CreateTransaction();
            _uow.Setup(u => u.Reviews).Returns(_reviews.Object);
            _uow.Setup(u => u.AppUserRepo).Returns(_users.Object);
            _uow.Setup(u => u.MenuItems).Returns(_menuItems.Object);
            _uow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_transaction.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<ReviewDetailsDto>>(_cache);
            _sut = new ReviewService(
                _uow.Object,
                _cache.Object,
                _mapper.Object,
                _logger.Object,
                _notifications.Object,
                _sendNotifications.Object);
        }

        [Fact]
        public async Task GetAllReviews_WhenCalled_UsesDetailsQuery()
        {
            var query = new List<Reviews>().AsQueryable();
            _reviews.Setup(r => r.GetReviewsWithDetails()).Returns(query);
            _reviews.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Reviews>(), 0));
            _mapper.Setup(m => m.Map<List<ReviewDetailsDto>>(It.IsAny<IEnumerable<Reviews>>()))
                .Returns(new List<ReviewDetailsDto>());

            var result = await _sut.GetAllReviews(1, 10);

            Assert.Empty(result.Data);
            _reviews.Verify(r => r.GetReviewsWithDetails(), Times.Once);
        }

        [Fact]
        public async Task GetReviewById_WhenReviewExists_ReturnsMappedDto()
        {
            var review = new Reviews { Id = 4 };
            var dto = new ReviewDetailsDto { Id = 4 };
            _reviews.Setup(r => r.GetReviewWithDetails(4)).ReturnsAsync(review);
            _mapper.Setup(m => m.Map<ReviewDetailsDto>(review)).Returns(dto);

            var result = await _sut.GetReviewById(4);

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetReviewById_WhenReviewIsMissing_ThrowsNotFoundException()
        {
            _reviews.Setup(r => r.GetReviewWithDetails(4)).ReturnsAsync((Reviews?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetReviewById(4));

            Assert.Equal("Review not found", ex.Message);
        }

        [Fact]
        public async Task GetMenuItemReviews_WhenCalled_UsesMenuItemQuery()
        {
            var query = new List<Reviews>().AsQueryable();
            _reviews.Setup(r => r.GetMenuItemReviews(8)).Returns(query);
            _reviews.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Reviews>(), 0));
            _mapper.Setup(m => m.Map<List<ReviewDetailsDto>>(It.IsAny<IEnumerable<Reviews>>()))
                .Returns(new List<ReviewDetailsDto>());

            await _sut.GetMenuItemReviews(8, 1, 10);

            _reviews.Verify(r => r.GetMenuItemReviews(8), Times.Once);
        }

        [Fact]
        public async Task GetUserReviews_WhenCalled_UsesUserQuery()
        {
            var query = new List<Reviews>().AsQueryable();
            _reviews.Setup(r => r.GetUserReviews("u1")).Returns(query);
            _reviews.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Reviews>(), 0));
            _mapper.Setup(m => m.Map<List<ReviewDetailsDto>>(It.IsAny<IEnumerable<Reviews>>()))
                .Returns(new List<ReviewDetailsDto>());

            await _sut.GetUserReviews("u1", 1, 10);

            _reviews.Verify(r => r.GetUserReviews("u1"), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("great")]
        public async Task SearchReviews_WhenCalled_UsesSearchQuery(string? search)
        {
            var query = new List<Reviews>().AsQueryable();
            _reviews.Setup(r => r.SearchReviews(search)).Returns(query);
            _reviews.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Reviews>(), 0));
            _mapper.Setup(m => m.Map<List<ReviewDetailsDto>>(It.IsAny<IEnumerable<Reviews>>()))
                .Returns(new List<ReviewDetailsDto>());

            await _sut.SearchReviews(search, 1, 10);

            _reviews.Verify(r => r.SearchReviews(search), Times.Once);
        }

        [Fact]
        public async Task CreateReview_WhenValid_CreatesReviewAndSendsNotifications()
        {
            var dto = new CreateReviewDto { MenuItemId = 8, Rating = 5, Comment = "Great" };
            var user = new AppUser { Id = "u1", UserName = "john" };
            var menuItem = new MenuItems { id = 8, name = "Burger" };
            var review = new Reviews { Id = 1 };
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(user);
            _menuItems.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(menuItem);
            _reviews.Setup(r => r.GetUserReviews("u1")).Returns(new List<Reviews>().BuildMock());
            _mapper.Setup(m => m.Map<Reviews>(dto)).Returns(review);
            _mapper.Setup(m => m.Map<ReviewDetailsDto>(review)).Returns(new ReviewDetailsDto { Id = 1 });

            var result = await _sut.CreateReview(dto, "u1");

            Assert.Equal(1, result.Id);
            Assert.Equal("u1", review.CustomerId);
            _reviews.Verify(r => r.CreateAsync(review), Times.Once);
            _notifications.Verify(n => n.CreateAsync(It.IsAny<CreateNotificationDto>()), Times.Once);
            _sendNotifications.Verify(s => s.SendToUserAsync("u1", It.IsAny<string>()), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Reviews"), Times.Once);
        }

        [Fact]
        public async Task CreateReview_WhenCustomerIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.CreateReview(new CreateReviewDto { MenuItemId = 8 }, "u1"));

            Assert.Equal("Customer not found", ex.Message);
        }

        [Fact]
        public async Task CreateReview_WhenMenuItemIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _menuItems.Setup(r => r.GetByIdAsync(8)).ReturnsAsync((MenuItems?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.CreateReview(new CreateReviewDto { MenuItemId = 8 }, "u1"));

            Assert.Equal("Menu item not found", ex.Message);
        }

        [Fact]
        public async Task CreateReview_WhenReviewAlreadyExists_ThrowsBadRequestException()
        {
            var existing = new Reviews
            {
                CustomerId = "u1",
                MenuItems = new MenuItems { id = 8 }
            };
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _menuItems.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(new MenuItems { id = 8, name = "Burger" });
            _reviews.Setup(r => r.GetUserReviews("u1")).Returns(new List<Reviews> { existing }.BuildMock());

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _sut.CreateReview(new CreateReviewDto { MenuItemId = 8 }, "u1"));

            Assert.Equal("Review Already Exist", ex.Message);
            _reviews.Verify(r => r.CreateAsync(It.IsAny<Reviews>()), Times.Never);
        }

        [Fact]
        public async Task UpdateReview_WhenOwnedReviewExists_UpdatesFieldsAndNotifies()
        {
            var review = new Reviews { Id = 4, CustomerId = "u1", Rating = 2, Comment = "old" };
            _reviews.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(review);

            await _sut.UpdateReview(4, new UpdateReviewDto { Rating = 5, Comment = "new" }, "u1");

            Assert.Equal(5, review.Rating);
            Assert.Equal("new", review.Comment);
            _reviews.Verify(r => r.UpdateAsync(review), Times.Once);
            _notifications.Verify(n => n.CreateAsync(It.IsAny<CreateNotificationDto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateReview_WhenReviewIsMissing_ThrowsNotFoundException()
        {
            _reviews.Setup(r => r.GetByIdAsync(4)).ReturnsAsync((Reviews?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateReview(4, new UpdateReviewDto(), "u1"));

            Assert.Equal("Review not found", ex.Message);
        }

        [Fact]
        public async Task UpdateReview_WhenReviewBelongsToAnotherCustomer_ThrowsNotFoundException()
        {
            _reviews.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(new Reviews { Id = 4, CustomerId = "other" });

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateReview(4, new UpdateReviewDto(), "u1"));

            Assert.Equal("Review not found", ex.Message);
        }

        [Fact]
        public async Task DeleteReview_WhenOwnedReviewExists_DeletesAndNotifies()
        {
            var review = new Reviews { Id = 4, CustomerId = "u1" };
            _reviews.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(review);

            await _sut.DeleteReview(4, "u1");

            _reviews.Verify(r => r.DeleteAsync(review), Times.Once);
            _notifications.Verify(n => n.CreateAsync(It.IsAny<CreateNotificationDto>()), Times.Once);
            _sendNotifications.Verify(s => s.SendToUserAsync("u1", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteReview_WhenReviewIsMissing_ThrowsNotFoundException()
        {
            _reviews.Setup(r => r.GetByIdAsync(4)).ReturnsAsync((Reviews?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteReview(4, "u1"));

            Assert.Equal("Review not found", ex.Message);
        }
    }
}
