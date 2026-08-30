using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Resturant_Ordering_System.Application.DTOs.CartDTOs;
using Resturant_Ordering_System.Application.Services;
using Resturant_Ordering_System.Domain.Abstract;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Services
{
    public class CartServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<ICartRepository> _carts = new();
        private readonly Mock<IMenuItemRepo> _menuItems = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILogger<CartService>> _logger = new();
        private readonly Mock<IDbContextTransaction> _transaction;
        private readonly CartService _sut;

        public CartServiceTests()
        {
            _transaction = TestMockHelpers.CreateTransaction();
            _uow.Setup(u => u.Cart).Returns(_carts.Object);
            _uow.Setup(u => u.MenuItems).Returns(_menuItems.Object);
            _uow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_transaction.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<GetCartDto>>(_cache);
            _sut = new CartService(_logger.Object, _uow.Object, _mapper.Object, _cache.Object);
        }

        [Fact]
        public async Task AddToCartAsync_WhenMenuItemIsMissing_ThrowsNotFoundException()
        {
            var dto = new AddToCartDto { MenuItemId = 8, Quantity = 1 };
            _menuItems.Setup(r => r.GetByIdAsync(8)).ReturnsAsync((MenuItems?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.AddToCartAsync("u1", dto));

            Assert.Equal("Menu item not found", ex.Message);
            _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddToCartAsync_WhenCartIsMissing_CreatesCartAndAddsItem()
        {
            var dto = new AddToCartDto { MenuItemId = 8, Quantity = 2 };
            var cartItem = new CartItem { MenuItemId = 8, Quantity = 2 };
            _menuItems.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(new MenuItems { id = 8 });
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync((Cart?)null);
            _mapper.Setup(m => m.Map<CartItem>(dto)).Returns(cartItem);

            await _sut.AddToCartAsync("u1", dto);

            _carts.Verify(r => r.CreateAsync(It.Is<Cart>(c => c.CustomerId == "u1")), Times.Once);
            _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Carts"), Times.Once);
        }

        [Fact]
        public async Task AddToCartAsync_WhenItemAlreadyInCart_IncrementsQuantity()
        {
            var dto = new AddToCartDto { MenuItemId = 8, Quantity = 2 };
            var existing = new CartItem { MenuItemId = 8, Quantity = 1 };
            var cart = new Cart { CustomerId = "u1", Items = new List<CartItem> { existing } };
            _menuItems.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(new MenuItems { id = 8 });
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync(cart);

            await _sut.AddToCartAsync("u1", dto);

            Assert.Equal(3, existing.Quantity);
            _mapper.Verify(m => m.Map<CartItem>(It.IsAny<AddToCartDto>()), Times.Never);
            _carts.Verify(r => r.CreateAsync(It.IsAny<Cart>()), Times.Never);
        }

        [Fact]
        public async Task GetCartAsync_WhenCartExists_ReturnsMappedDto()
        {
            var cart = new Cart { Id = 1, CustomerId = "u1" };
            var dto = new GetCartDto { CartId = 1 };
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync(cart);
            _mapper.Setup(m => m.Map<GetCartDto>(cart)).Returns(dto);

            var result = await _sut.GetCartAsync("u1");

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetCartAsync_WhenCartIsMissing_ThrowsNotFoundException()
        {
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync((Cart?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetCartAsync("u1"));

            Assert.Equal("Cart not found", ex.Message);
        }

        [Fact]
        public async Task GetAllCartsAsync_WhenCalled_UsesItemsQuery()
        {
            var query = new List<Cart>().AsQueryable();
            _carts.Setup(r => r.GetAllWithItems()).Returns(query);
            _carts.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Cart>(), 0));
            _mapper.Setup(m => m.Map<List<GetCartDto>>(It.IsAny<IEnumerable<Cart>>()))
                .Returns(new List<GetCartDto>());

            var result = await _sut.GetAllCartsAsync(1, 10);

            Assert.Empty(result.Data);
            _carts.Verify(r => r.GetAllWithItems(), Times.Once);
        }

        [Fact]
        public async Task ClearCartAsync_WhenCartExists_ClearsItemsAndClearsCache()
        {
            var cart = new Cart
            {
                CustomerId = "u1",
                Items = new List<CartItem> { new() { Id = 1 }, new() { Id = 2 } }
            };
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync(cart);

            await _sut.ClearCartAsync("u1");

            Assert.Empty(cart.Items);
            _cache.Verify(c => c.RemoveAsync("Get_Carts"), Times.Once);
        }

        [Fact]
        public async Task ClearCartAsync_WhenCartIsMissing_ThrowsNotFoundException()
        {
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync((Cart?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.ClearCartAsync("u1"));

            Assert.Equal("Cart not found", ex.Message);
        }

        [Fact]
        public async Task RemoveCartItemAsync_WhenItemExists_RemovesItem()
        {
            var item = new CartItem { Id = 4 };
            var cart = new Cart { CustomerId = "u1", Items = new List<CartItem> { item } };
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync(cart);

            await _sut.RemoveCartItemAsync("u1", 4);

            Assert.Empty(cart.Items);
            _cache.Verify(c => c.RemoveAsync("Get_Carts"), Times.Once);
        }

        [Fact]
        public async Task RemoveCartItemAsync_WhenCartIsMissing_ThrowsNotFoundException()
        {
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync((Cart?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.RemoveCartItemAsync("u1", 4));

            Assert.Equal("Cart not found", ex.Message);
        }

        [Fact]
        public async Task RemoveCartItemAsync_WhenItemIsMissing_ThrowsNotFoundException()
        {
            var cart = new Cart { CustomerId = "u1" };
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync(cart);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.RemoveCartItemAsync("u1", 4));

            Assert.Equal("Cart item not found", ex.Message);
        }

        [Fact]
        public async Task UpdateCartItemAsync_WhenItemExists_UpdatesQuantity()
        {
            var item = new CartItem { Id = 4, Quantity = 1 };
            var cart = new Cart { CustomerId = "u1", Items = new List<CartItem> { item } };
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync(cart);

            await _sut.UpdateCartItemAsync("u1", 4, new UpdateCartDto { Quantity = 5 });

            Assert.Equal(5, item.Quantity);
            _cache.Verify(c => c.RemoveAsync("Get_Carts"), Times.Once);
        }

        [Fact]
        public async Task UpdateCartItemAsync_WhenCartIsMissing_ThrowsNotFoundException()
        {
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync((Cart?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateCartItemAsync("u1", 4, new UpdateCartDto { Quantity = 2 }));

            Assert.Equal("Cart not found", ex.Message);
        }

        [Fact]
        public async Task UpdateCartItemAsync_WhenItemIsMissing_ThrowsNotFoundException()
        {
            _carts.Setup(r => r.GetCartWithItemsAsync("u1")).ReturnsAsync(new Cart { CustomerId = "u1" });

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateCartItemAsync("u1", 4, new UpdateCartDto { Quantity = 2 }));

            Assert.Equal("Cart item not found", ex.Message);
        }
    }
}
