using Application.DTOs.FavoriteDTOs;
using Application.Services;
using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Services
{
    public class FavoriteServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<IFavoriteRepo> _favorites = new();
        private readonly Mock<IGenaricRepo<AppUser, string>> _users = new();
        private readonly Mock<IMenuItemRepo> _menuItems = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILogger<FavoriteService>> _logger = new();
        private readonly Mock<IDbContextTransaction> _transaction;
        private readonly FavoriteService _sut;

        public FavoriteServiceTests()
        {
            _transaction = TestMockHelpers.CreateTransaction();
            _uow.Setup(u => u.FavoriteRepo).Returns(_favorites.Object);
            _uow.Setup(u => u.AppUserRepo).Returns(_users.Object);
            _uow.Setup(u => u.MenuItems).Returns(_menuItems.Object);
            _uow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_transaction.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<GetFavoriteDto>>(_cache);
            _sut = new FavoriteService(_uow.Object, _cache.Object, _mapper.Object, _logger.Object);
        }

        [Fact]
        public async Task GetAllFavoritesAsync_WhenCalled_ReturnsMappedPage()
        {
            _favorites.Setup(r => r.GetAllPaged(1, 10))
                .ReturnsAsync((new List<Favorite>().AsEnumerable(), 0));
            _mapper.Setup(m => m.Map<List<GetFavoriteDto>>(It.IsAny<IEnumerable<Favorite>>()))
                .Returns(new List<GetFavoriteDto>());

            var result = await _sut.GetAllFavoritesAsync(1, 10);

            Assert.Equal(1, result.PageNumber);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetFavoriteByIdAsync_WhenFavoriteExists_ReturnsMappedDto()
        {
            var favorite = new Favorite { Id = 3 };
            var dto = new GetFavoriteDto { Id = 3 };
            _favorites.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(favorite);
            _mapper.Setup(m => m.Map<GetFavoriteDto>(favorite)).Returns(dto);

            var result = await _sut.GetFavoriteByIdAsync(3);

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetFavoriteByIdAsync_WhenFavoriteIsMissing_ThrowsNotFoundException()
        {
            _favorites.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((Favorite?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetFavoriteByIdAsync(3));

            Assert.Equal("Favorite not found", ex.Message);
        }

        [Fact]
        public async Task AddFavoriteAsync_WhenValid_CreatesFavoriteAndClearsCache()
        {
            var dto = new GetFavoriteDto { CustomerId = "u1", MenuItemId = 8 };
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _menuItems.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(new MenuItems { id = 8 });
            _favorites.Setup(r => r.GetFavoriteByCustomerAndMenuItemAsync("u1", 8))
                .ReturnsAsync((Favorite?)null);
            _mapper.Setup(m => m.Map<GetFavoriteDto>(It.IsAny<Favorite>())).Returns(dto);

            var result = await _sut.AddFavoriteAsync("u1", 8);

            Assert.Same(dto, result);
            _favorites.Verify(r => r.CreateAsync(It.Is<Favorite>(f => f.CustomerId == "u1" && f.MenuItemId == 8)), Times.Once);
            _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Favorites"), Times.Once);
        }

        [Fact]
        public async Task AddFavoriteAsync_WhenCustomerIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.AddFavoriteAsync("u1", 8));

            Assert.Equal("Customer not found", ex.Message);
            _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddFavoriteAsync_WhenMenuItemIsMissing_ThrowsNotFoundException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _menuItems.Setup(r => r.GetByIdAsync(8)).ReturnsAsync((MenuItems?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.AddFavoriteAsync("u1", 8));

            Assert.Equal("Menu item not found", ex.Message);
        }

        [Fact]
        public async Task AddFavoriteAsync_WhenFavoriteAlreadyExists_ThrowsBadRequestException()
        {
            _users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new AppUser { Id = "u1" });
            _menuItems.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(new MenuItems { id = 8 });
            _favorites.Setup(r => r.GetFavoriteByCustomerAndMenuItemAsync("u1", 8))
                .ReturnsAsync(new Favorite { Id = 1 });

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.AddFavoriteAsync("u1", 8));

            Assert.Equal("Favorite already exists", ex.Message);
            _favorites.Verify(r => r.CreateAsync(It.IsAny<Favorite>()), Times.Never);
        }

        [Fact]
        public async Task DeleteFavoriteAsync_WhenFavoriteExists_DeletesAndClearsCache()
        {
            var favorite = new Favorite { Id = 4, CustomerId = "u1" };
            _favorites.Setup(r => r.GetFavoriteByIdAndCustomerIdAsync(4, "u1")).ReturnsAsync(favorite);

            await _sut.DeleteFavoriteAsync(4, "u1");

            _favorites.Verify(r => r.DeleteAsync(favorite), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Favorites"), Times.Once);
        }

        [Fact]
        public async Task DeleteFavoriteAsync_WhenFavoriteIsMissing_ThrowsNotFoundException()
        {
            _favorites.Setup(r => r.GetFavoriteByIdAndCustomerIdAsync(4, "u1")).ReturnsAsync((Favorite?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteFavoriteAsync(4, "u1"));

            Assert.Equal("Favorite not found", ex.Message);
        }

        [Fact]
        public async Task SearchFavoritesAsync_WhenCalled_UsesSearchQuery()
        {
            var query = new List<Favorite>().AsQueryable();
            _favorites.Setup(r => r.SearchFavoritesAsync("pizza")).Returns(query);
            _favorites.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Favorite>(), 0));
            _mapper.Setup(m => m.Map<List<GetFavoriteDto>>(It.IsAny<IEnumerable<Favorite>>()))
                .Returns(new List<GetFavoriteDto>());

            var result = await _sut.SearchFavoritesAsync("pizza", 1, 10);

            Assert.Empty(result.Data);
            _favorites.Verify(r => r.SearchFavoritesAsync("pizza"), Times.Once);
        }

        [Fact]
        public async Task GetFavoritesByCategoryAsync_WhenCalled_UsesCategoryQuery()
        {
            var query = new List<Favorite>().AsQueryable();
            _favorites.Setup(r => r.GetFavoritesByCategoryAsync(2)).Returns(query);
            _favorites.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Favorite>(), 0));
            _mapper.Setup(m => m.Map<List<GetFavoriteDto>>(It.IsAny<IEnumerable<Favorite>>()))
                .Returns(new List<GetFavoriteDto>());

            await _sut.GetFavoritesByCategoryAsync(2, 1, 10);

            _favorites.Verify(r => r.GetFavoritesByCategoryAsync(2), Times.Once);
        }

        [Fact]
        public async Task GetFavoritesByMenuItemAsync_WhenCalled_UsesMenuItemQuery()
        {
            var query = new List<Favorite>().AsQueryable();
            _favorites.Setup(r => r.GetFavoritesByMenuItemsAsync(8)).Returns(query);
            _favorites.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Favorite>(), 0));
            _mapper.Setup(m => m.Map<List<GetFavoriteDto>>(It.IsAny<IEnumerable<Favorite>>()))
                .Returns(new List<GetFavoriteDto>());

            await _sut.GetFavoritesByMenuItemAsync(8, 1, 10);

            _favorites.Verify(r => r.GetFavoritesByMenuItemsAsync(8), Times.Once);
        }

        [Fact]
        public async Task GetFavoritesByCustomerIdAsync_WhenCalled_UsesCustomerQuery()
        {
            var query = new List<Favorite>().AsQueryable();
            _favorites.Setup(r => r.GetFavoritesByCustomerIdAsync("u1")).Returns(query);
            _favorites.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Favorite>(), 0));
            _mapper.Setup(m => m.Map<List<GetFavoriteDto>>(It.IsAny<IEnumerable<Favorite>>()))
                .Returns(new List<GetFavoriteDto>());

            await _sut.GetFavoritesByCustomerIdAsync("u1", 1, 10);

            _favorites.Verify(r => r.GetFavoritesByCustomerIdAsync("u1"), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IsFavoriteExistsAsync_WhenChecked_ReturnsWhetherFavoriteExists(bool exists)
        {
            _favorites.Setup(r => r.GetFavoriteByCustomerAndMenuItemAsync("u1", 8))
                .ReturnsAsync(exists ? new Favorite { Id = 1 } : null);

            var result = await _sut.IsFavoriteExistsAsync("u1", 8);

            Assert.Equal(exists, result);
        }

        [Fact]
        public async Task RemoveByMenuItemIdAsync_WhenFavoriteExists_DeletesFavorite()
        {
            var favorite = new Favorite { Id = 1, CustomerId = "u1", MenuItemId = 8 };
            _favorites.Setup(r => r.GetFavoriteByCustomerAndMenuItemAsync("u1", 8)).ReturnsAsync(favorite);

            await _sut.RemoveByMenuItemIdAsync("u1", 8);

            _favorites.Verify(r => r.DeleteAsync(favorite), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveByMenuItemIdAsync_WhenFavoriteIsMissing_ThrowsNotFoundException()
        {
            _favorites.Setup(r => r.GetFavoriteByCustomerAndMenuItemAsync("u1", 8)).ReturnsAsync((Favorite?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.RemoveByMenuItemIdAsync("u1", 8));

            Assert.Equal("favourite Not Found", ex.Message);
            _favorites.Verify(r => r.DeleteAsync(It.IsAny<Favorite>()), Times.Never);
        }
    }
}
